using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.UI {
    public class ExplicitButton : Button {
        [SerializeField] private Selectable m_Left, m_Right, m_Up, m_Down;

        public Selectable left { get => m_Left; set => m_Left = value; }
        public Selectable right { get => m_Right; set => m_Right = value; }
        public Selectable up { get => m_Up; set => m_Up = value; }
        public Selectable down { get => m_Down; set => m_Down = value; }

        public override Selectable FindSelectableOnLeft() {
            return left ? left : base.FindSelectableOnLeft();
        }

        public override Selectable FindSelectableOnRight() {
            return right ? right : base.FindSelectableOnRight();
        }

        public override Selectable FindSelectableOnUp() {
            return up ? up : base.FindSelectableOnUp();
        }

        public override Selectable FindSelectableOnDown() {
            return down ? down : base.FindSelectableOnDown();
        }
    }
}