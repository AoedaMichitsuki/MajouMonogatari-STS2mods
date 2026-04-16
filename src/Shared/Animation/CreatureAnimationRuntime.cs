using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MajouMonogatari_STS2mods.Characters.Cecily;
using MajouMonogatari_STS2mods.Shared.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MajouMonogatari_STS2mods.Shared.Animation;

public static class CreatureAnimationRuntime
{
    public static class AnimationNames
    {
        public const string Idle = "Idle";
        public const string Hit = "Hit";
        public const string Attack = "Attack";
        public const string Cast = "Cast";
        public const string Dead = "Dead";
    }

    public readonly struct AnimationStep
    {
        public AnimationStep(
            string animationName,
            int delayMs = 0,
            bool fromEnd = false,
            bool returnToIdle = true,
            bool forceRestart = true)
        {
            AnimationName = animationName;
            DelayMs = Math.Max(0, delayMs);
            FromEnd = fromEnd;
            ReturnToIdle = returnToIdle;
            ForceRestart = forceRestart;
        }

        public string AnimationName { get; }
        public int DelayMs { get; }
        public bool FromEnd { get; }
        public bool ReturnToIdle { get; }
        public bool ForceRestart { get; }
    }

    private readonly struct AnimationRoute
    {
        public AnimationRoute(IReadOnlyList<string> candidates, bool fromEnd, bool returnToIdle)
        {
            Candidates = candidates;
            FromEnd = fromEnd;
            ReturnToIdle = returnToIdle;
        }

        public IReadOnlyList<string> Candidates { get; }
        public bool FromEnd { get; }
        public bool ReturnToIdle { get; }
    }

    private sealed class CardPlayContext
    {
        public string CardEntryId;
    }

    private static readonly string[] IdleCandidates = [AnimationNames.Idle];
    private static readonly string[] HitCandidates = [AnimationNames.Hit];
    private static readonly string[] AttackCandidates = [AnimationNames.Attack];
    private static readonly string[] CastCandidates = [AnimationNames.Cast, AnimationNames.Attack];
    private static readonly string[] DeadCandidates = [AnimationNames.Dead];

    private static readonly AnimationRoute DefaultRoute = new(IdleCandidates, false, false);
    private static readonly Dictionary<string, AnimationRoute> BuiltInRouteByTrigger = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Hit"] = new AnimationRoute(HitCandidates, false, true),
        ["Attack"] = new AnimationRoute(AttackCandidates, false, true),
        ["Cast"] = new AnimationRoute(CastCandidates, false, true),
        ["Dead"] = new AnimationRoute(DeadCandidates, false, false)
    };

    private static readonly Dictionary<string, AnimationRoute> CustomRouteByTrigger = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Dictionary<string, AnimationRoute>> CardRouteByCardAndTrigger = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> SuppressedTriggers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, HashSet<string>> SuppressedCardTriggers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, AnimationStep[]> CardPlaySequenceByCard = new(StringComparer.OrdinalIgnoreCase);
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Creature, CardPlayContext> CardContextByCreature = new();

    public static bool TryHandleEngineTrigger(NCreature creatureNode, string trigger)
    {
        var modelId = creatureNode?.Entity?.ModelId?.ToString();
        var cardEntryId = GetCurrentCardEntryId(creatureNode?.Entity);
        var isTargetCreature = IsCecilyPlayerCreature(creatureNode, cardEntryId);
        ModLog.Info(
            $"Animation trigger observed: trigger='{trigger ?? "<null>"}', " +
            $"modelId='{modelId ?? "<null>"}', card='{cardEntryId ?? "<null>"}', isTarget={isTargetCreature}.");

        if (!isTargetCreature)
        {
            return false;
        }

        var route = ResolveRoute(creatureNode.Entity, trigger, out var suppressed);
        if (suppressed)
        {
            ModLog.Info($"Animation trigger suppressed: trigger='{trigger ?? "<null>"}'.");
            return false;
        }

        var played = TryPlayRoute(creatureNode, route);
        ModLog.Info(
            $"Animation trigger handled: trigger='{trigger ?? "<null>"}', " +
            $"candidates=[{string.Join(", ", route.Candidates ?? Array.Empty<string>())}], " +
            $"played={played}.");
        return played;
    }

    public static bool TryPlayCustom(
        Creature creature,
        string animationName,
        bool fromEnd = false,
        bool returnToIdle = true,
        bool forceRestart = true)
    {
        if (creature == null || string.IsNullOrWhiteSpace(animationName))
        {
            return false;
        }

        if (!TryFindCreatureNode(creature, out var creatureNode))
        {
            ModLog.Warn($"Creature node not found for custom animation '{animationName}'.");
            return false;
        }

        var route = new AnimationRoute([animationName], fromEnd, returnToIdle);
        return TryPlayRoute(creatureNode, route, forceRestart);
    }

    public static bool TryPlayCustomForCardOwner(
        CardModel card,
        string animationName,
        bool fromEnd = false,
        bool returnToIdle = true,
        bool forceRestart = true)
    {
        return TryPlayCustom(card?.Owner?.Creature, animationName, fromEnd, returnToIdle, forceRestart);
    }

    public static void RegisterTriggerRoute(
        string trigger,
        IReadOnlyList<string> candidates,
        bool fromEnd = false,
        bool returnToIdle = true)
    {
        if (string.IsNullOrWhiteSpace(trigger) || candidates == null || candidates.Count == 0)
        {
            return;
        }

        CustomRouteByTrigger[trigger] = new AnimationRoute(candidates, fromEnd, returnToIdle);
    }

    public static void RegisterCardTriggerRoute(
        string cardEntryId,
        string trigger,
        IReadOnlyList<string> candidates,
        bool fromEnd = false,
        bool returnToIdle = true)
    {
        if (string.IsNullOrWhiteSpace(cardEntryId) || string.IsNullOrWhiteSpace(trigger) || candidates == null || candidates.Count == 0)
        {
            return;
        }

        if (!CardRouteByCardAndTrigger.TryGetValue(cardEntryId, out var routeByTrigger))
        {
            routeByTrigger = new Dictionary<string, AnimationRoute>(StringComparer.OrdinalIgnoreCase);
            CardRouteByCardAndTrigger[cardEntryId] = routeByTrigger;
        }

        routeByTrigger[trigger] = new AnimationRoute(candidates, fromEnd, returnToIdle);
    }

    public static void RegisterCardPlaySequence(string cardEntryId, IReadOnlyList<AnimationStep> steps)
    {
        if (string.IsNullOrWhiteSpace(cardEntryId) || steps == null || steps.Count == 0)
        {
            return;
        }

        var copied = new AnimationStep[steps.Count];
        for (var i = 0; i < steps.Count; i++)
        {
            copied[i] = steps[i];
        }

        CardPlaySequenceByCard[cardEntryId] = copied;
    }

    public static void SuppressTrigger(string trigger)
    {
        if (string.IsNullOrWhiteSpace(trigger))
        {
            return;
        }

        SuppressedTriggers.Add(trigger);
    }

    public static void UnsuppressTrigger(string trigger)
    {
        if (string.IsNullOrWhiteSpace(trigger))
        {
            return;
        }

        SuppressedTriggers.Remove(trigger);
    }

    public static void SuppressCardTrigger(string cardEntryId, string trigger)
    {
        if (string.IsNullOrWhiteSpace(cardEntryId) || string.IsNullOrWhiteSpace(trigger))
        {
            return;
        }

        if (!SuppressedCardTriggers.TryGetValue(cardEntryId, out var triggerSet))
        {
            triggerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SuppressedCardTriggers[cardEntryId] = triggerSet;
        }

        triggerSet.Add(trigger);
    }

    public static void UnsuppressCardTrigger(string cardEntryId, string trigger)
    {
        if (string.IsNullOrWhiteSpace(cardEntryId) || string.IsNullOrWhiteSpace(trigger))
        {
            return;
        }

        if (!SuppressedCardTriggers.TryGetValue(cardEntryId, out var triggerSet))
        {
            return;
        }

        triggerSet.Remove(trigger);
        if (triggerSet.Count == 0)
        {
            SuppressedCardTriggers.Remove(cardEntryId);
        }
    }

    public static void ClearTriggerRoute(string trigger)
    {
        if (string.IsNullOrWhiteSpace(trigger))
        {
            return;
        }

        CustomRouteByTrigger.Remove(trigger);
        SuppressedTriggers.Remove(trigger);
    }

    public static void ClearCardAnimationConfig(string cardEntryId)
    {
        if (string.IsNullOrWhiteSpace(cardEntryId))
        {
            return;
        }

        CardRouteByCardAndTrigger.Remove(cardEntryId);
        SuppressedCardTriggers.Remove(cardEntryId);
        CardPlaySequenceByCard.Remove(cardEntryId);
    }

    public static void ClearAllCustomConfigs()
    {
        CustomRouteByTrigger.Clear();
        CardRouteByCardAndTrigger.Clear();
        SuppressedTriggers.Clear();
        SuppressedCardTriggers.Clear();
        CardPlaySequenceByCard.Clear();
    }

    public static void BeginCardPlay(CardPlay cardPlay)
    {
        var card = cardPlay?.Card;
        var ownerCreature = card?.Owner?.Creature;
        var cardEntryId = card?.Id.Entry;

        if (ownerCreature == null || string.IsNullOrWhiteSpace(cardEntryId))
        {
            return;
        }

        var context = CardContextByCreature.GetOrCreateValue(ownerCreature);
        context.CardEntryId = cardEntryId;

        if (CardPlaySequenceByCard.TryGetValue(cardEntryId, out var steps))
        {
            _ = PlaySequenceAsync(ownerCreature, steps);
        }
    }

    public static void EndCardPlay(CardPlay cardPlay)
    {
        var ownerCreature = cardPlay?.Card?.Owner?.Creature;
        if (ownerCreature == null)
        {
            return;
        }

        if (CardContextByCreature.TryGetValue(ownerCreature, out var context))
        {
            context.CardEntryId = null;
        }
    }

    private static AnimationRoute ResolveRoute(Creature creature, string trigger, out bool suppressed)
    {
        suppressed = false;

        if (string.IsNullOrWhiteSpace(trigger))
        {
            return DefaultRoute;
        }

        var cardId = GetCurrentCardEntryId(creature);
        if (!string.IsNullOrWhiteSpace(cardId) &&
            SuppressedCardTriggers.TryGetValue(cardId, out var triggerSet) &&
            triggerSet.Contains(trigger))
        {
            suppressed = true;
            return DefaultRoute;
        }

        if (SuppressedTriggers.Contains(trigger))
        {
            suppressed = true;
            return DefaultRoute;
        }

        if (!string.IsNullOrWhiteSpace(cardId) &&
            CardRouteByCardAndTrigger.TryGetValue(cardId, out var routeByTrigger) &&
            routeByTrigger.TryGetValue(trigger, out var cardRoute))
        {
            return cardRoute;
        }

        if (CustomRouteByTrigger.TryGetValue(trigger, out var customRoute))
        {
            return customRoute;
        }

        if (BuiltInRouteByTrigger.TryGetValue(trigger, out var builtInRoute))
        {
            return builtInRoute;
        }

        return DefaultRoute;
    }

    private static bool TryPlayRoute(NCreature creatureNode, AnimationRoute route, bool forceRestart = true)
    {
        if (creatureNode == null)
        {
            return false;
        }

        if (TryFindFirstDescendant(creatureNode, out AnimationPlayer animationPlayer) &&
            TryPlayWithAnimationPlayer(animationPlayer, route, forceRestart))
        {
            return true;
        }

        if (TryFindFirstDescendant(creatureNode, out AnimatedSprite2D sprite) &&
            TryPlayWithAnimatedSprite(sprite, route, forceRestart))
        {
            return true;
        }

        ModLog.Warn($"No animation component found on creature node '{creatureNode.Name}'.");
        return false;
    }

    private static bool TryPlayWithAnimationPlayer(AnimationPlayer animationPlayer, AnimationRoute route, bool forceRestart)
    {
        if (animationPlayer == null)
        {
            return false;
        }

        var playName = ResolveAnimationName(
            route.Candidates,
            candidate => animationPlayer.HasAnimation(candidate),
            () => FirstAnimationName(animationPlayer.GetAnimationList()));

        if (string.IsNullOrWhiteSpace(playName))
        {
            return false;
        }

        if (!forceRestart &&
            string.Equals(animationPlayer.CurrentAnimation, playName, StringComparison.OrdinalIgnoreCase) &&
            animationPlayer.IsPlaying())
        {
            return true;
        }

        animationPlayer.Play(playName, -1d, 1f, route.FromEnd);

        if (!route.ReturnToIdle)
        {
            return true;
        }

        var idleName = ResolveAnimationName(
            IdleCandidates,
            candidate => animationPlayer.HasAnimation(candidate),
            () => FirstAnimationName(animationPlayer.GetAnimationList()));

        if (!string.IsNullOrWhiteSpace(idleName) &&
            !string.Equals(playName, idleName, StringComparison.OrdinalIgnoreCase))
        {
            animationPlayer.Queue(idleName);
        }

        return true;
    }

    private static bool TryPlayWithAnimatedSprite(AnimatedSprite2D sprite, AnimationRoute route, bool forceRestart)
    {
        if (sprite?.SpriteFrames == null)
        {
            return false;
        }

        var frames = sprite.SpriteFrames;
        var playName = ResolveAnimationName(
            route.Candidates,
            candidate => frames.HasAnimation(candidate),
            () => FirstAnimationName(frames.GetAnimationNames()));

        if (string.IsNullOrWhiteSpace(playName))
        {
            return false;
        }

        if (!forceRestart &&
            string.Equals(sprite.Animation.ToString(), playName, StringComparison.OrdinalIgnoreCase) &&
            sprite.IsPlaying())
        {
            return true;
        }

        sprite.Frame = 0;
        sprite.Play(playName, 1f, route.FromEnd);

        if (!route.ReturnToIdle)
        {
            return true;
        }

        var idleName = ResolveAnimationName(
            IdleCandidates,
            candidate => frames.HasAnimation(candidate),
            () => FirstAnimationName(frames.GetAnimationNames()));

        if (string.IsNullOrWhiteSpace(idleName) ||
            string.Equals(playName, idleName, StringComparison.OrdinalIgnoreCase) ||
            frames.GetAnimationLoop(playName))
        {
            return true;
        }

        sprite.Connect(
            AnimatedSprite2D.SignalName.AnimationFinished,
            Callable.From(() =>
            {
                if (!GodotObject.IsInstanceValid(sprite))
                {
                    return;
                }

                if (sprite.SpriteFrames == null || !sprite.SpriteFrames.HasAnimation(idleName))
                {
                    return;
                }

                sprite.Play(idleName);
            }),
            4u);

        return true;
    }

    private static bool IsCecilyPlayerCreature(NCreature creatureNode, string currentCardEntryId = null)
    {
        if (creatureNode?.Entity == null || !creatureNode.Entity.IsPlayer)
        {
            return false;
        }

        var modelId = creatureNode.Entity.ModelId?.ToString();
        if (!string.IsNullOrWhiteSpace(modelId) &&
            modelId.EndsWith(CecilyIds.Character, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(currentCardEntryId) &&
               currentCardEntryId.StartsWith(CecilyIds.Character + "_", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCurrentCardEntryId(Creature creature)
    {
        if (creature == null)
        {
            return null;
        }

        return CardContextByCreature.TryGetValue(creature, out var context)
            ? context.CardEntryId
            : null;
    }

    private static bool TryFindCreatureNode(Creature creature, out NCreature creatureNode)
    {
        creatureNode = null;
        if (creature == null)
        {
            return false;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return false;
        }

        return TryFindCreatureNodeRecursive(tree.Root, creature, out creatureNode);
    }

    private static bool TryFindCreatureNodeRecursive(Node node, Creature creature, out NCreature creatureNode)
    {
        creatureNode = null;
        if (node == null)
        {
            return false;
        }

        if (node is NCreature nCreature && ReferenceEquals(nCreature.Entity, creature))
        {
            creatureNode = nCreature;
            return true;
        }

        var childCount = node.GetChildCount();
        for (var i = 0; i < childCount; i++)
        {
            var child = node.GetChild(i);
            if (!TryFindCreatureNodeRecursive(child, creature, out creatureNode))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool TryFindFirstDescendant<T>(Node root, out T found) where T : Node
    {
        found = null;
        if (root == null)
        {
            return false;
        }

        if (root is T resolved)
        {
            found = resolved;
            return true;
        }

        var childCount = root.GetChildCount();
        for (var i = 0; i < childCount; i++)
        {
            var child = root.GetChild(i);
            if (!TryFindFirstDescendant(child, out found))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static string ResolveAnimationName(
        IReadOnlyList<string> candidates,
        Func<string, bool> exists,
        Func<string> fallbackFactory)
    {
        if (candidates != null)
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (string.IsNullOrWhiteSpace(candidate) || !exists(candidate))
                {
                    continue;
                }

                return candidate;
            }
        }

        var fallback = fallbackFactory();
        if (string.IsNullOrWhiteSpace(fallback) || !exists(fallback))
        {
            return null;
        }

        return fallback;
    }

    private static string FirstAnimationName(IEnumerable names)
    {
        if (names == null)
        {
            return null;
        }

        foreach (var raw in names)
        {
            var resolved = raw?.ToString();
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }
        }

        return null;
    }

    private static async Task PlaySequenceAsync(Creature creature, IReadOnlyList<AnimationStep> steps)
    {
        if (creature == null || steps == null || steps.Count == 0)
        {
            return;
        }

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            if (step.DelayMs > 0)
            {
                await Delay(step.DelayMs);
            }

            TryPlayCustom(
                creature,
                step.AnimationName,
                fromEnd: step.FromEnd,
                returnToIdle: step.ReturnToIdle,
                forceRestart: step.ForceRestart);
        }
    }

    private static async Task Delay(int milliseconds)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        if (Engine.GetMainLoop() is SceneTree tree)
        {
            var timer = tree.CreateTimer(milliseconds / 1000d);
            await tree.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
            return;
        }

        await Task.Delay(milliseconds);
    }
}
