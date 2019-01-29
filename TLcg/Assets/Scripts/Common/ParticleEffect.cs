using UnityEngine;
using System.Collections.Generic;

namespace LCG
{
    public class ParticleEffect : MonoBehaviour
    {
        private List<Particle> m_particles = new List<Particle>();
        private bool m_initialize = false;

        private void Awake()
        {
            if (m_initialize)
            {
                return;
            }
            m_initialize = true;
            SearchParticles(transform);
        }

        public void Scaler(float f)
        {
            foreach (var v in m_particles)
            {
                v.Scaler(f);
            }
        }

        public void Speedup(float f)
        {
            foreach (var v in m_particles)
            {
                v.Speedup(f);
            }
        }

        private void SearchParticles(Transform target)
        {
            ParticleSystem v = target.GetComponent<ParticleSystem>();
            if (null != v)
            {
                m_particles.Add(new Particle(v));
            }

            for (int i = 0; i < target.childCount; i++)
            {
                SearchParticles(target.GetChild(i));
            }
        }

        public class Particle
        {
            public ParticleSystem TheParticle { get; private set; }
            public float SimulationSpeed { get; private set; }
            public float StartSizeMultiplier { get; private set; }

            public Particle(ParticleSystem particle)
            {
                TheParticle = particle;
                SimulationSpeed = particle.main.simulationSpeed;
                StartSizeMultiplier = particle.main.startSizeMultiplier;
            }
            public void Scaler(float f)
            {
                var main = TheParticle.main;
                main.startSizeMultiplier = StartSizeMultiplier * f;
            }
            public void Speedup(float f)
            {
                var main = TheParticle.main;
                main.simulationSpeed = SimulationSpeed * f;
            }
        }
    }
}