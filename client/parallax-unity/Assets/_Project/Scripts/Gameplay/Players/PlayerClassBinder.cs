using System;
using System.Collections;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Characters;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace Parallax.Gameplay.Players
{
    /// <summary>
    /// Liga o jogador à classe que ele ocupa no SessionRoster: troca o modelo, a habilidade, a cápsula,
    /// o perfil de câmera e, no dono, o Olhar da câmera local. Ao nascer, o dono pede a primeira classe livre.
    /// </summary>
    public sealed class PlayerClassBinder : NetworkBehaviour
    {
        [Serializable]
        public struct ClassBody
        {
            public ParallaxClass parallaxClass;
            public ClassDefinition definition;
            public CameraProfile cameraProfile;
            public GameObject model;
            public Behaviour ability;
            public float capsuleHeight;
            public float capsuleRadius;
            public float eyeHeight;
        }

        [SerializeField] private ClassBody[] bodies = new ClassBody[0];
        [SerializeField] private CharacterController controller;
        [SerializeField] private ParallaxCharacterMotor motor;
        [SerializeField] private ParallaxInput input;
        [SerializeField] private CharacterAnimator characterAnimator;
        [SerializeField] private Transform cameraTarget;
        [SerializeField, Tooltip("Afastamento lateral entre jogadores ao nascer, em metros.")]
        private float spawnSpacing = 3f;

        private SessionRoster _roster;
        private bool _modelHidden;

        public static PlayerClassBinder Local { get; private set; }

        public event Action<ParallaxClass> ClassApplied;

        public bool HasClass { get; private set; }
        public ParallaxClass CurrentClass { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Local = null;

        private void Awake()
        {
            foreach (ClassBody body in bodies)
            {
                if (body.model != null)
                {
                    body.model.SetActive(false);
                }

                if (body.ability != null)
                {
                    body.ability.enabled = false;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            input.enabled = IsOwner;
            if (IsOwner)
            {
                Local = this;
                MoveToSpawnSlot();
                ParallaxCameraRig rig = ParallaxCameraRig.Active;
                if (rig != null)
                {
                    rig.Bind(cameraTarget, cameraTarget);
                }
            }

            StartCoroutine(BindToRoster());
        }

        public override void OnNetworkDespawn()
        {
            if (_roster != null)
            {
                _roster.RosterChanged -= Sync;
            }

            if (Local == this)
            {
                Local = null;
            }
        }

        /// <summary>Pede outra classe. O servidor recusa se outro humano já a ocupa.</summary>
        public void RequestClass(ParallaxClass parallaxClass)
        {
            if (IsOwner && _roster != null)
            {
                _roster.RequestClass(parallaxClass);
            }
        }

        /// <summary>Na 1ª pessoa, o próprio modelo só projeta sombra, para não tapar a visão.</summary>
        public void SetLocalModelHidden(bool hidden)
        {
            _modelHidden = hidden;
            foreach (ClassBody body in bodies)
            {
                if (body.model == null)
                {
                    continue;
                }

                foreach (Renderer meshRenderer in body.model.GetComponentsInChildren<Renderer>(true))
                {
                    meshRenderer.shadowCastingMode = hidden ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
                }
            }
        }

        private IEnumerator BindToRoster()
        {
            while (SessionRoster.Active == null)
            {
                yield return null;
            }

            _roster = SessionRoster.Active;
            _roster.RosterChanged += Sync;
            Sync();
        }

        private void Sync()
        {
            if (_roster.TryGetClassOf(OwnerClientId, out ParallaxClass claimed))
            {
                if (!HasClass || claimed != CurrentClass)
                {
                    Apply(claimed);
                }

                return;
            }

            if (IsOwner && !HasClass)
            {
                RequestFirstFreeClass();
            }
        }

        private void RequestFirstFreeClass()
        {
            for (int i = 0; i < ParallaxRoles.ClassCount; i++)
            {
                var candidate = (ParallaxClass)i;
                if (!_roster.IsHuman(candidate))
                {
                    _roster.RequestClass(candidate);
                    return;
                }
            }
        }

        private void Apply(ParallaxClass parallaxClass)
        {
            ClassBody selected = default;
            foreach (ClassBody body in bodies)
            {
                bool active = body.parallaxClass == parallaxClass;
                if (body.model != null)
                {
                    body.model.SetActive(active);
                }

                if (body.ability != null)
                {
                    body.ability.enabled = active;
                }

                if (active)
                {
                    selected = body;
                }
            }

            controller.height = selected.capsuleHeight;
            controller.radius = selected.capsuleRadius;
            controller.center = new Vector3(0f, selected.capsuleHeight * 0.5f, 0f);
            cameraTarget.localPosition = new Vector3(0f, selected.eyeHeight, 0f);
            motor.SetClassDefinition(selected.definition);
            characterAnimator.Use(parallaxClass);

            if (IsOwner)
            {
                ParallaxCameraRig rig = ParallaxCameraRig.Active;
                if (rig != null)
                {
                    rig.ApplyProfile(selected.cameraProfile);
                    if (rig.Vision != null)
                    {
                        rig.Vision.SetViewer(parallaxClass);
                    }
                }

                SetLocalModelHidden(_modelHidden);
            }

            CurrentClass = parallaxClass;
            HasClass = true;
            ClassApplied?.Invoke(parallaxClass);
        }

        private void MoveToSpawnSlot()
        {
            int slot = (int)(OwnerClientId % (ulong)ParallaxRoles.ClassCount);
            Vector3 position;
            Quaternion rotation = transform.rotation;
            if (PlayerSpawnPoint.Active != null)
            {
                position = PlayerSpawnPoint.Active.SlotPosition(slot);
                rotation = PlayerSpawnPoint.Active.Facing;
            }
            else
            {
                position = transform.position + new Vector3((slot - 1) * spawnSpacing, 0.05f, 0f);
            }

            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
        }
    }
}
