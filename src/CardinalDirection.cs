using Godot;

namespace TreeTrunk
{
    public enum CardinalDirection
    {
        Left, Right, Up, Down
    }

    public static class CardinalDirectionExtension
    {
        public static CardinalDirection FromVector(Vector2 v)
        {
            if (v.X > 0 && (Mathf.Abs(v.X) > Mathf.Abs(v.Y)))
            {
                return CardinalDirection.Right;
            }
            else if (v.X < 0 && (Mathf.Abs(v.X) > Mathf.Abs(v.Y)))
            {
                return CardinalDirection.Left;
            }
            else if (v.Y < 0 && (Mathf.Abs(v.Y) > Mathf.Abs(v.X)))
            {
                return CardinalDirection.Up;
            }
            else
            {
                return CardinalDirection.Down;
            }
        }

        public static float RotationDegreesFromRight(this CardinalDirection direction)
        {
            switch (direction)
            {
                case CardinalDirection.Left:
                    return 180;

                case CardinalDirection.Right:
                    return 0;

                case CardinalDirection.Up:
                    return 270;

                default:
                    return 90;
            }
        }
    }
}
