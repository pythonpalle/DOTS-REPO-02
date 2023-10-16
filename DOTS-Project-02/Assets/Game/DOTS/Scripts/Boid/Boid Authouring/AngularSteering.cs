namespace DOTS
{
    [System.Serializable]  
    public struct AngularSteering
    {
        public float weight;
        public float maxAngularAcceleration;
        public float maxRotation;
        public float targetRadius;
        public float slowRadius;
        public float timeToTarget; 
    }
}