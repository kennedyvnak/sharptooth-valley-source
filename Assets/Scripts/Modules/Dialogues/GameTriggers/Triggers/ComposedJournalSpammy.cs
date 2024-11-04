using NFHGame.SpammyEvents;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedJournalSpammy : GameTriggerBase {
        public override bool Match(string id) {
            return id switch {
                "dinnerGoesAhead" => true,
                "trustPointsMadness" => true,
                "bastheetGoesInvestigate" => true,
                "spammyDrawsBow" => true,
                "spamBastSettle" => true,
                "spamBastFire" => true,
                "bastRedEye" => true,
                "bastTurnsEvil" => true,
                "headacheStart" => true,
                "headacheStop" => true,
                "secondImpact" => true,
                "spammyJournalLeaves" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            switch (id) {
                case "dinnerGoesAhead":
                    JournalSpammyBattle.instance.DinnerGoesAhead(handler);
                    return true;
                case "trustPointsMadness":
                    JournalSpammyBattle.instance.TrustPointsMadness(handler);
                    return true;
                case "bastheetGoesInvestigate":
                    JournalSpammyBattle.instance.BastheetGoesInvestigate(handler);
                    return true;
                case "spammyDrawsBow":
                    JournalSpammyBattle.instance.SpammyDrawsBow(handler);
                    return true;
                case "spamBastSettle":
                    JournalSpammyBattle.instance.SpammyBastSettle(handler);
                    return true;
                case "spamBastFire":
                    JournalSpammyBattle.instance.SpamBastFire(handler);
                    return true;
                case "headacheStart":
                    JournalSpammyBattle.instance.HeadacheStart(handler);
                    return true;
                case "headacheStop":
                    JournalSpammyBattle.instance.HeadacheStop(handler);
                    return true;
                case "bastRedEye":
                    JournalSpammyBattle.instance.BastRedEye(handler);
                    return true;
                case "bastTurnsEvil":
                    JournalSpammyBattle.instance.BastTurnsEvil(handler);
                    return true;
                case "secondImpact":
                    JournalSpammyBattle.instance.SecondImpact(handler);
                    return true;
                case "spammyJournalLeaves":
                    JournalSpammyBattle.instance.SpammyJournalLeaves(handler);
                    return true;
                default:
                    return false;
            }
        }
    }
}
