using Crimson_Knight_Server.Monsters;
using Crimson_Knight_Server.Npcs;
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
        public short Id {  get; set; }
        public string Name { get; set; }
        public short XEnter { get; set; }
        public short YEnter { get; set; }

        public List<Monster> Monsters = new List<Monster>();
        public List<Npc> Npcs = new List<Npc>();
        public List<Player> Players = new List<Player>();

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
            if(template.Npcs != null)
            {
                for (int i = 0; i < template.Npcs.Count; i++)
                {
                    var item = template.Npcs[i];
                    var npc = new Npc(item.X, item.Y, TemplateManager.NpcTemplates[item.TemplateId]);
                    Npcs.Add(npc);
                }
            }
        }


        public void UpdateMap()
        {
           
        }
    }
}
