using NFHGame.SpammyEvents;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedArrowSpammy : GameTriggerBase {
        public override bool Match(string id) {
            return id switch {
                "spammyReveal" => true,
                "dinnerShifts" => true,
                "surpriseMoment" => true,
                "arrowGameOver" => true,
                "spammyLeaves" => true,
                "spammyJoins" => true,
                "amnesiaHamster" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            switch (id) {
                case "spammyReveal":
                    ArrowSpammyBattle.instance.SpammyReveal(handler);
                    return true;
                case "dinnerShifts":
                    ArrowSpammyBattle.instance.DinnerShifts(handler);
                    return true;
                case "surpriseMoment":
                    ArrowSpammyBattle.instance.SurpriseMoment(handler);
                    return true;
                case "arrowGameOver":
                    ArrowSpammyBattle.instance.ArrowGameOver(handler);
                    return true;
                case "spammyLeaves":
                    ArrowSpammyBattle.instance.SpammyLeaves(handler);
                    return true;
                case "spammyJoins":
                    ArrowSpammyBattle.instance.SpammyJoins(handler);
                    return true;
                case "amnesiaHamster":
                    ArrowSpammyBattle.instance.AmensiaHamster(handler);
                    return true;
                default:
                    return false;
            };
        }
    }
}
