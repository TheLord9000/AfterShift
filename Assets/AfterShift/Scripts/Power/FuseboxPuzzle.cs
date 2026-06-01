using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Runtime;
using UHFPS.Tools;

namespace AfterShift.Power
{
    [RequireComponent(typeof(AudioSource))]
    public class ASFuseboxPuzzle : PuzzleBaseSimple, IInventorySelector
    {
        [Serializable]
        public sealed class FuseElement
        {
            public GameObject FuseObject;
            public Light FuseLight;
            public MeshRenderer LightRenderer;
            public bool IsInserted;
            public bool IsBroken;
        }

        [Header("Items")]
        public ItemProperty FuseItem;
        public ItemProperty BrokenFuseItem;

        [Header("Fusebox")]
        public bool UseInteract = false;
        public List<FuseElement> Fuses = new();

        [Header("Colors")]
        public bool UseFuseColors = true;
        public string EmissionKeyword = "_EMISSION";
        public string EmissionColorName = "_EmissionColor";
        public string BaseColorName = "_BaseColor";
        public Color InsertedFuseColor = Color.green;
        public Color NoFuseColor = Color.white;
        public Color BrokenFuseColor = Color.red;

        [Header("Sounds")]
        public SoundClip FuseInsertSound;
        public SoundClip FusesConnectedSound;

        [Header("Events")]
        public UnityEvent OnAllFusesConnected;
        public UnityEvent OnFusesDisconnected;
        public UnityEvent<int> OnFuseConnected;
        public UnityEvent<int> OnFuseBroken;
        public UnityEvent<int> OnBrokenFusesRemoved;

        public bool FusesConnected { get; private set; }

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            foreach (var fuse in Fuses)
            {
                RefreshFuseVisual(fuse);
            }

            RefreshConnectedState(false);
        }

        public override void InteractStart()
        {
            if (HasBrokenFuses())
            {
                RemoveAllBrokenFuses();
                return;
            }

            if (HasMissingFuses())
            {
                if (!UseInteract)
                {
                    Inventory.Instance.OpenItemSelector(this);
                }
                else
                {
                    InventoryItem fuseItem = Inventory.Instance.GetInventoryItem(FuseItem);
                    if (fuseItem != null)
                        InsertFuse(fuseItem);
                }

                return;
            }

            // Tutto ok: nessuna interazione.
        }

        public bool CanInteractWithFusebox()
        {
            return HasBrokenFuses() || HasMissingFuses();
        }

        public bool ShouldInsertFuses()
        {
            return !HasBrokenFuses() && HasMissingFuses();
        }

        public bool ShouldRemoveBrokenFuses()
        {
            return HasBrokenFuses();
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (selectedItem.ItemGuid != FuseItem)
                return;

            InsertFuse(selectedItem);
        }

        public bool HasMissingFuses()
        {
            return Fuses.Any(x => !x.IsInserted);
        }

        public bool HasBrokenFuses()
        {
            return Fuses.Any(x => x.IsBroken);
        }

        public bool AreAllFusesWorking()
        {
            return Fuses.Count > 0 && Fuses.All(x => x.IsInserted && !x.IsBroken);
        }

        private void InsertFuse(InventoryItem fuseItem)
        {
            int quantity = Mathf.Clamp(fuseItem.Quantity, 0, Fuses.Count);
            int inserted = 0;

            for (int i = 0; i < quantity; i++)
            {
                FuseElement emptyFuse = Fuses.FirstOrDefault(x => !x.IsInserted && !x.IsBroken);

                if (emptyFuse == null)
                    break;

                SetFuseInserted(emptyFuse, true);
                OnFuseConnected?.Invoke(Fuses.IndexOf(emptyFuse));
                inserted++;
            }

            if (inserted <= 0)
                return;

            audioSource.PlayOneShotSoundClip(FuseInsertSound);
            Inventory.Instance.RemoveItem(fuseItem, (ushort)inserted);

            RefreshConnectedState(true);
        }

        public void BurnRandomFuses(int amount)
        {
            List<FuseElement> availableFuses = Fuses
                .Where(x => x.IsInserted && !x.IsBroken)
                .ToList();

            int amountToBurn = Mathf.Clamp(amount, 0, availableFuses.Count);

            for (int i = 0; i < amountToBurn; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableFuses.Count);
                FuseElement selectedFuse = availableFuses[randomIndex];

                BreakFuse(selectedFuse);

                availableFuses.RemoveAt(randomIndex);
            }

            RefreshConnectedState(true);
        }

        public void BurnOneRandomFuse()
        {
            BurnRandomFuses(1);
        }

        public void RemoveAllBrokenFuses()
        {
            int removed = 0;

            foreach (FuseElement fuse in Fuses)
            {
                if (!fuse.IsBroken)
                    continue;

                fuse.IsBroken = false;
                fuse.IsInserted = false;

                if (fuse.FuseObject != null)
                    fuse.FuseObject.SetActive(false);

                RefreshFuseVisual(fuse);
                removed++;
            }

            if (removed > 0)
            {
                GiveBrokenFusesToPlayer((ushort)removed);
                OnBrokenFusesRemoved?.Invoke(removed);
                RefreshConnectedState(true);
            }
        }

        private void GiveBrokenFusesToPlayer(ushort amount)
        {
            if (BrokenFuseItem == null)
            {
                Debug.LogWarning("BrokenFuseItem is not assigned.");
                return;
            }

            Inventory.Instance.AddItem(BrokenFuseItem.GUID, amount, null);
        }

        private void BreakFuse(FuseElement fuse)
        {
            fuse.IsBroken = true;
            fuse.IsInserted = true;

            RefreshFuseVisual(fuse);

            int index = Fuses.IndexOf(fuse);
            OnFuseBroken?.Invoke(index);
        }

        private void SetFuseInserted(FuseElement fuse, bool inserted)
        {
            fuse.IsInserted = inserted;

            if (!inserted)
                fuse.IsBroken = false;

            if (fuse.FuseObject != null)
                fuse.FuseObject.SetActive(inserted);

            RefreshFuseVisual(fuse);
        }

        private void RefreshConnectedState(bool invokeEvents)
        {
            bool nowConnected = AreAllFusesWorking();

            if (FusesConnected == nowConnected)
                return;

            FusesConnected = nowConnected;

            if (!invokeEvents)
                return;

            if (FusesConnected)
            {
                audioSource.PlayOneShotSoundClip(FusesConnectedSound);
                OnAllFusesConnected?.Invoke();
            }
            else
            {
                OnFusesDisconnected?.Invoke();
            }
        }

        private void RefreshFuseVisual(FuseElement fuse)
        {
            if (fuse == null)
                return;

            bool visible = fuse.IsInserted || fuse.IsBroken;

            if (fuse.FuseObject != null)
                fuse.FuseObject.SetActive(visible);

            if (fuse.LightRenderer != null)
            {
                Material material = fuse.LightRenderer.material;

                if (visible)
                    material.EnableKeyword(EmissionKeyword);
                else
                    material.DisableKeyword(EmissionKeyword);

                if (UseFuseColors)
                {
                    Color color = GetFuseColor(fuse);
                    material.SetColor(EmissionColorName, color);
                    material.SetColor(BaseColorName, color);
                }
            }

            if (fuse.FuseLight != null)
            {
                fuse.FuseLight.enabled = visible;

                if (UseFuseColors)
                    fuse.FuseLight.color = GetFuseColor(fuse);
            }
        }

        private Color GetFuseColor(FuseElement fuse)
        {
            if (fuse.IsBroken)
                return BrokenFuseColor;

            if (fuse.IsInserted)
                return InsertedFuseColor;

            return NoFuseColor;
        }
    }
}