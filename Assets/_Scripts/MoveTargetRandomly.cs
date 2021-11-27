using System;
using UnityEngine;

namespace UnityStandardAssets.SceneUtils
{
    public class MoveTargetRandomly : MonoBehaviour
    {
        public int minimumWaitTime = 3;
        public int maximumWaitTime = 6;
        public int minimumDistance = 3;
        public int maximumDistance = 7;
        [Tooltip("Maximum distance from starting position object will move.")]
        public int maximumTotalDistance = 20;
        public float surfaceOffset = 1.5f;
        public GameObject setTargetOn;
        private int pauseDuration;
        private float lastMoved;
        private bool setPosition = false;
        private Vector3 startingPosition;

        private void Start()
        {
            lastMoved = Time.time;
            CalculateNextPauseDuration();
            transform.position = setTargetOn.transform.position;
            startingPosition = setTargetOn.transform.position;
        }

        private void CalculateNextPauseDuration()
        {
            pauseDuration = UnityEngine.Random.Range(minimumWaitTime, maximumWaitTime);
        }

        private bool CanMove()
        {
            return Time.time - lastMoved >= pauseDuration;
        }

        private float direction2Angle(int direction)
        {
            float angle = 0;
            switch (direction)
            {
                case 0: // north west
                    angle = 3 * Mathf.PI / 4;
                    break;
                case 1: // north
                    angle = Mathf.PI / 2;
                    break;
                case 2: // north east
                    angle = Mathf.PI / 4;
                    break;
                case 3: // east
                    angle = 2 * Mathf.PI;
                    break;
                case 4: // south east
                    angle = 3 * Mathf.PI / 4;
                    break;
                case 5: // south
                    angle = 3 * Mathf.PI / 2;
                    break;
                case 6: // south west
                    angle = 7 * Mathf.PI / 4;
                    break;
                case 7: // west
                    angle = Mathf.PI;
                    break;
            }
            return angle;
        }

        private void RandomPosition()
        {
            float distanceToMove = UnityEngine.Random.Range(minimumDistance, maximumDistance);
            float directionAngle = 0;

            float newDistance = Vector3.Distance(setTargetOn.transform.position, startingPosition) + distanceToMove;
            Vector3 destination;
            if (newDistance > maximumTotalDistance)
            {
                 destination = startingPosition;
            }
            else
            {
                int randomDirection = (int)UnityEngine.Random.Range(0, 8);
                directionAngle = direction2Angle(randomDirection);

                destination = new Vector3
                {
                    x = transform.position.x + distanceToMove * Mathf.Cos(directionAngle),
                    z = transform.position.z + distanceToMove * Mathf.Sin(directionAngle),
                    y = transform.position.y
                };
            }

            setTargetOn.SendMessage("SetTargetPosition", destination);
        }

        // Update is called once per frame
        private void Update()
        {
            if (CanMove())
            {
                if (!setPosition)
                {
                    RandomPosition();
                    // setTargetOn.SendMessage("SetTarget", transform);
                    setPosition = true;
                }

                if (Vector3.Distance(transform.position, setTargetOn.transform.position) <= 0.2)
                {
                    CalculateNextPauseDuration();
                    lastMoved = Time.time;
                    setPosition = false;
                }
            }
        }
    }
}
