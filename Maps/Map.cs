using Crimson_Knight_Server.Monsters;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils.Loggings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crimson_Knight_Server.Maps
{
    public class Map
    {
        public readonly ConcurrentQueue<Player> BusPlayerEnterMap = new ConcurrentQueue<Player>();
        public readonly ConcurrentQueue<Player> BusPlayerExitMap = new ConcurrentQueue<Player>();


        public short Id {  get; set; }
        public string Name { get; set; }
        //public MapTemplate Template;
        public short XEnter { get; set; }
        public short YEnter { get; set; }

        public List<Monster> Monsters = new List<Monster>();

        public Map(MapTemplate template)
        {
            this.Id = template.Id;
            this.Name = template.Name;
            this.XEnter = template.XEnter;
            this.YEnter = template.YEnter;
            if (template.Monsters != null)
            {
                for(int i = 0;i<template.Monsters.Count;i++)
                {
                    var item = template.Monsters[i];
                    var monster = new Monster(i, item.X, item.Y, TemplateManager.MonsterTemplates[item.TemplateId]);
                    Monsters.Add(monster);
                }
            }
        }

        public List<Player> Players = new List<Player>();

        private void PlayerEnterMap(Player player)
        {
            Players.Add(player);
            player.MapCur = this;
            player.SendEnterMap();
            player.SendOtherPlayersInMap();
            player.SendMonstersInMap();
            player.BroadcastEnterMap();
        }
        private void PlayerExitMap(Player player)
        {
            Players.Remove(player);
            player.BroadcastExitMap();
            player.MapCur = null;
        }
        public void UpdateMap()
        {
            while (BusPlayerExitMap.TryDequeue(out Player playerExit))
            {
                PlayerExitMap(playerExit);
            }
            while (BusPlayerEnterMap.TryDequeue(out Player playerEnter))
            {
                PlayerEnterMap(playerEnter);
                //ConsoleLogging.LogInfor(
                //                        $"GetMaxHp() {playerEnter.GetMaxHp()}, " +
                //                        $"GetMaxMp() {playerEnter.GetMaxMp()}, " +
                //                        $"GetAtk() {playerEnter.GetAtk()}, " +
                //                        $"GetDef() {playerEnter.GetDef()}"
                //                        );
                ConsoleLogging.LogInfor($"Player {playerEnter.PlayerId} đã vào map {Id}");
            }
        }
    }
}
