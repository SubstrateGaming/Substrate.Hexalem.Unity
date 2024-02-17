﻿using Assets.Polkadot_Unity_SDK.DemoGame.Scripts;
using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using System;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using static Substrate.NetApi.Mnemonic;

namespace Assets.Scripts.ScreenStates
{
    public class ImportSeedState: GameBaseState
    {
        private Button _btnCreateWalletSeed;

        private HexalemTextField _txfSeedPhrase;

        public ImportSeedState(DemoGameController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("BottomBound");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>("DemoGame/UI/Elements/ImportSeedElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWalletSeed = instance.Q<Button>("BtnCreateWalletSeed");
            _btnCreateWalletSeed.SetEnabled(false);
            _btnCreateWalletSeed.RegisterCallback<ClickEvent>(OnClickBtnCreateWalletSeed);
            var _lblFillRandom = instance.Q<Label>("LblFillRandom");
            _lblFillRandom.RegisterCallback<ClickEvent>(OnClickLblFillRandom);
            _txfSeedPhrase = instance.Q<HexalemTextField>("TxfSeedPhrase");
            _txfSeedPhrase.TextField.RegisterValueChangedCallback(OnChangeEventSeedPhrase);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Current, StepState.None, StepState.None);

            var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            var imgLogo = Resources.Load<Texture2D>($"DemoGame/Icons/IconSeedPhrase");
            velLogo.style.backgroundImage = imgLogo;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            if (FlowController.CurrentState == DemoGameScreen.UnlockWallet)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void OnClickBtnCreateWalletSeed(ClickEvent evt)
        {
            FlowController.TempAccount = Network.Keyring.AddFromUri(FlowController.TempMnemonic, new Meta(), KeyType.Sr25519).Account;

            Debug.Log($"Temporary account {FlowController.TempAccount} stored with keytype {FlowController.TempAccount.KeyType}");
            FlowController.ChangeScreenState(DemoGameScreen.CreateWallet);
        }

        private void OnClickLblFillRandom(ClickEvent evt)
        {
            var randomBytes = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            _txfSeedPhrase.TextField.value = string.Join(" ", Mnemonic.MnemonicFromEntropy(randomBytes, Mnemonic.BIP39Wordlist.English));
        }

        private void OnChangeEventSeedPhrase(ChangeEvent<string> evt)
        {
            _btnCreateWalletSeed.SetEnabled(false);

            var words = evt.newValue.Split(' ');
            if (words.Length >= 12 && words.All(p => p.Length > 2))
            {
                FlowController.TempMnemonic = evt.newValue;
                _btnCreateWalletSeed.SetEnabled(true);
            } else
            {
                Debug.Log("Invalid seed phrase");
            }
        }
    }
}