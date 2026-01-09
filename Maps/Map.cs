using Crimson_Knight_Server.Maps.MessageMap.Attack;
using Crimson_Knight_Server.Monsters;
using Crimson_Knight_Server.Networking;
using Crimson_Knight_Server.Npcs;
using Crimson_Knight_Server.Players;
using Crimson_Knight_Server.Templates;
using Crimson_Knight_Server.Utils;
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
        public ConcurrentBag<AttackMessage> AttackMessages = new ConcurrentBag<AttackMessage>();

        public short Id { get; set; }
        public string Name { get; set; }
        public short XEnter { get; set; }
        public short YEnter { get; set; }

        public List<Monster> Monsters = new List<Monster>();
        public List<Npc> Npcs = new List<Npc>();
        public List<Player> Players = new List<Player>();

        public Player GetPlayerById(int playerId)
        {
            return Players.FirstOrDefault(p => p.Id == playerId);
        }
        public Map(MapTemplate template)
        {
            this.Id = template.Id;
            this.Name = template.Name;
            this.XEnter = template.XEnter;
            this.YEnter = template.YEnter;
            if (template.Monsters != null)
            {
                for (int i = 0; i < template.Monsters.Count; i++)
                {
                    var item = template.Monsters[i];
                    var monster = new Monster(i, item.X, item.Y, TemplateManager.MonsterTemplates[item.TemplateId]);
                    Monsters.Add(monster);
                }
            }
            if (template.Npcs != null)
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
            HandleAttackMessages();
        }

        private void HandleAttackMessages()
        {
            while (AttackMessages.TryTake(out var attackMessage))
            {
                try
                {
                    Player playerSend = GetPlayerById(attackMessage.PlayerSenderId);
                    if (playerSend == null)
                    {
                        continue;
                    }
                    Skill skillUse = playerSend.Skills[attackMessage.SkillUseId];
                    if (!skillUse.IsLearned || !skillUse.CanAttack(playerSend.CurrentMp))
                    {
                        continue;
                    }
                    skillUse.StartTimeAttack = SystemUtil.CurrentTimeMillis();
                    playerSend.CurrentMp -= skillUse.GetMpLost();

                    int countTarget = 0;
                    List<BaseObject> objTakeDamages = new List<BaseObject>();
                    for (int i = 0; i < attackMessage.TargetIds.Length; i++)
                    {
                        if (countTarget >= skillUse.GetTargetCount())
                        {
                            break;
                        }
                        countTarget++;
                        BaseObject objReceive = null;
                        int objectTargetId = attackMessage.TargetIds[i];
                        bool isPlayer = attackMessage.IsPlayers[i];
                        if (isPlayer)
                        {
                            objReceive = GetPlayerById(objectTargetId);
                            if (objReceive == null || objReceive.IsDie())
                            {
                                continue;
                            }
                        }
                        else
                        {
                            objReceive = Monsters[objectTargetId];
                            if (objReceive.IsDie())
                            {
                                continue;
                            }
                        }
                        int dam = playerSend.GetAtk();
                        dam = dam - objReceive.GetDef();
                        if (dam < 1)
                        {
                            dam = 1;
                        }
                        objReceive.TakeDamage(dam);
                        objTakeDamages.Add(objReceive);


                        foreach (var p in Players)
                        {
                            foreach (var item in objTakeDamages)
                            {
                                if (item.IsMonster())
                                {
                                    ((Monster)item).SendMonsterBaseInfo(p);
                                }
                            }
                        }
                        SendAttackPlayerInfoMsg(playerSend, skillUse.TemplateId, dam, objTakeDamages);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLogging.LogError("HandleAttackMessages" + ex.Message);
                }
            }
        }

        void SendAttackPlayerInfoMsg(BaseObject attacker, int skillUseId, int dame, List<BaseObject> targets)
        {
            Message msg = new Message(MessageId.SERVER_ALL_SEND_ATTACK_PLAYER_INFO);
            msg.WriteInt(attacker.Id);
            msg.WriteInt(skillUseId);
            msg.WriteInt(dame);

            msg.WriteByte((byte)targets.Count);
            for(int i =  0; i < targets.Count; i++)
            {
                msg.WriteBool(targets[i].IsPlayer());
                msg.WriteInt(targets[i].Id);
            }
            foreach(var p in Players)
            {
                p.SendMessage(msg);
            }
            msg.Close();
        }
    }
}
