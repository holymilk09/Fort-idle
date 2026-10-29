using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public class Palette
    {
        public Material Grass;
        public Material Dirt;
        public Material HeroBody;
        public Material HeroHead;
        public Material Axe;
        public Material Plank;
        public Material Trunk;
        public Material Canopy;
        public Material Stump;
        public Material Wall;
        public Material WallReinforced;
        public Material WallStone;
        public Material Rock;
        public Material Stone;
        public Material Iron;
        public Material Spike;
        public Material Turret;
        public Material TurretAccent;
        public Material Monster;
        public Material Eye;
        public Material Brute;
        public Material BruteHorn;
        public Material Fire;
        public Material FireLog;
        public Material Bolt;
        public Material Coin;
        public Material HutBody;
        public Material HutRoof;
        public Material HutDoor;
        public Material WorkerBody;
        public Material WorkerHead;
        public Material LockedGate;
        public Material LockedPost;
        public Material Meat;
        public Material Bone;
        public Material Hide;
        public Material Core;
        public Material Sap;
        public Material Boar;
        public Font Font;
        public Sprite Circle;
    }

    public static class FortWorld
    {
        public static FortIdleConfig Config;
        public static Palette Look;
        public static Transform Hero;
        public static PlankStack Stack;
        public static BuildPad Wall;
        public static BuildPad TurretPad;
        public static BuildPad Hut;
        public static BuildPad Spike;
        public static BuildPad Gate;
        public static BuildPad Tower;
        public static BuildPad TurretUpgrade;
        public static Turret TurretWeapon;
        public static Transform Trader;
        public static AssignPad StoneAssign;
        public static CoinPad StackPad;
        public static CoinPad SpeedPad;
        public static CoinPad HirePad;
        public static CoinPad MeleePad;
        public static GameHud Hud;
        public static Stockpile Stock;
        public static FireCore Fire;
        public static Camera Camera;
        public static FollowCamera Rig;
        public static Transform GroundRoot;
        public static Transform StaticRoot;
        public static MatchDirector Match;
        public static FortProgress Progress;
        public static readonly List<TreeNode> Trees = new List<TreeNode>();
        public static readonly List<Transform> Plants = new List<Transform>();
        public static readonly List<RockNode> Rocks = new List<RockNode>();
        public static readonly List<BuildPad> Pads = new List<BuildPad>();
        public static readonly List<Monster> Monsters = new List<Monster>();
        public static readonly List<CoinPickup> Coins = new List<CoinPickup>();
        public static readonly List<WorkerAgent> Workers = new List<WorkerAgent>();
        public static readonly List<Boar> Boars = new List<Boar>();

        public static void Reset()
        {
            Trees.Clear();
            Plants.Clear();
            Rocks.Clear();
            Pads.Clear();
            Monsters.Clear();
            Coins.Clear();
            Workers.Clear();
            Config = null;
            Look = null;
            Hero = null;
            Stack = null;
            Wall = null;
            TurretPad = null;
            Hut = null;
            Spike = null;
            Gate = null;
            Tower = null;
            TurretUpgrade = null;
            TurretWeapon = null;
            Trader = null;
            StoneAssign = null;
            StackPad = null;
            SpeedPad = null;
            HirePad = null;
            MeleePad = null;
            Hud = null;
            Boars.Clear();
            Stock = null;
            Fire = null;
            Camera = null;
            Rig = null;
            GroundRoot = null;
            StaticRoot = null;
            Match = null;
            Progress = null;
        }
    }
}
