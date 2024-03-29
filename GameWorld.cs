﻿using System;
using System.Collections.Generic;
using MazeGamePillaPilla.PowerUps;

namespace MazeGamePillaPilla
{
    class GameWorld
    {
        public Cell[,] maze;
        public Dictionary<string, Pj> Pjs { get; private set; }
        public Dictionary<int, Drop> Drops { get; private set; }
        public Dictionary<int, TintaSplash> TintaSplashes { get; private set; }
        public DateTime GameStartedTime;

        public GameWorld()
        {
            Pjs = new Dictionary<string, Pj>();
            Drops = new Dictionary<int, Drop>();
            TintaSplashes = new Dictionary<int, TintaSplash>();
        }


        public void OnCharacterUpdated(object source, GameplayUpdateEventArgs args)
        {
            Pjs[args.Packet.CharacterID].ProcessServerUpdate(args.Packet, maze);
        }

        public void OnDropAdded(object source, GameplayDropEventArgs args)
        {
            Drop drop = null;
            switch (args.Type)
            {
                case (int)DropTypes.SurpriseBoxDrop:
                    drop = new SurpriseBoxDrop(args.X, args.Y);
                    break;

                case (int)DropTypes.BananaDrop:
                    drop = new BananaDrop(args.X, args.Y);
                    break;

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException();
            }

            Drops.Add(args.Id, drop);
        }

        public void OnDropRemoved(object source, GameplayDropEventArgs args)
        {
            Drops.Remove(args.Id);
        }

        public void OnBuffAdded(object source, GameplayBuffEventArgs args)
        {
            if (Pjs.TryGetValue(args.PlayerId, out Pj pj))
            {
                switch (args.BuffType)
                {
                    case (int)BuffTypes.SprintBuff:
                        pj.Buffs.Add(args.BuffId, new SprintBuff(pj));
                        break;

                    case (int)BuffTypes.TraverseWallsBuff:
                        pj.Buffs.Add(args.BuffId, new TraverseWallsBuff(pj));
                        break;

                    case (int)BuffTypes.BananaStunBuff:
                        pj.Buffs.Add(args.BuffId, new BananaStunBuff(pj));
                        break;

                    case (int)BuffTypes.InvisibleBuff:
                        pj.Buffs.Add(args.BuffId, new InvisibleBuff(pj));
                        break;

                    case (int)BuffTypes.ImmuneBuff:
                        pj.Buffs.Add(args.BuffId, new ImmuneBuff(pj));
                        break;

                    case (int)BuffTypes.RelojBuff:
                        pj.Buffs.Add(args.BuffId, new RelojBuff(pj));
                        break;

                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException();
                }
            }
        }

        public void OnBuffRemoved(object source, GameplayBuffEventArgs args)
        {
            if (Pjs.TryGetValue(args.PlayerId, out Pj pj))
            {
                if (pj.Buffs.TryGetValue(args.BuffId, out Buff buff))
                {
                    buff.End();
                    pj.Buffs.Remove(args.BuffId);
                }
                else throw new System.ComponentModel.InvalidEnumArgumentException();
            }
            else throw new System.ComponentModel.InvalidEnumArgumentException();
        }

        public void OnPowerUpAdded(object source, GameplayPowerUpEventArgs args)
        {
            if (Pjs.TryGetValue(args.PlayerId, out Pj pj))
            {
                switch (args.Type)
                {
                    case (int)PowerUpTypes.SprintPowerUp:
                        pj.PowerUp = new SprintPowerUp();
                        break;

                    case (int)PowerUpTypes.TraverseWallsPowerUp:
                        pj.PowerUp = new TraverseWallsPowerUp();
                        break;

                    case (int)PowerUpTypes.BananaPowerUp:
                        pj.PowerUp = new BananaPowerUp();
                        break;

                    case (int)PowerUpTypes.InvisiblePowerUp:
                        pj.PowerUp = new InvisiblePowerUp();
                        break;

                    case (int)PowerUpTypes.TintaPowerUp:
                        pj.PowerUp = new TintaPowerUp();
                        break;

                    case (int)PowerUpTypes.ImmunePowerUp:
                        pj.PowerUp = new ImmunePowerUp();
                        break;

                    case (int)PowerUpTypes.RandomTeleportPowerUp:
                        pj.PowerUp = new RandomTeleportPowerUp();
                        break;

                    case (int)PowerUpTypes.RelojPowerUp:
                        pj.PowerUp = new RelojPowerUp();
                        break;

                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException();
                }
            }
        }

        public void OnPowerUpRemoved(object source, GameplayPowerUpEventArgs args)
        {
            if (Pjs.TryGetValue(args.PlayerId, out Pj pj))
            {
                pj.PowerUp = null;
            }
            else throw new System.ComponentModel.InvalidEnumArgumentException();
        }

        public void OnCharacterTeleported(object source, GameplayCharacterTeleportedEventArgs args)
        {
            if (Pjs.TryGetValue(args.PlayerId, out Pj pj))
            {
                pj.SetPosition(args.X, args.Y);
            }
            else throw new System.ComponentModel.InvalidEnumArgumentException();
        }

        public void OnTintaSplashAdded(object source, GameplayTintaSplashEventArgs args)
        {
            TintaSplashes.Add(args.Id, new TintaSplash(args.X, args.Y, args.Rotation, args.Duration));
        }

        public void OnTintaSplashRemoved(object source, GameplayTintaSplashEventArgs args)
        {
            TintaSplashes.Remove(args.Id);
        }
    }
}
