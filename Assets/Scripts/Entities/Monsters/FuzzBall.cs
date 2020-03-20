using Assets.Scripts.Monsters;
using Assets.Scripts.Player.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Entities.Monsters
{
    public class FuzzBall : Monster
    {

        public FuzzBall()
        {
            this.name += " FuzzBall";
            this.setId(MonsterTypes.FUZZBALL);
        }



        public override int getTarget()
        {
            throw new NotImplementedException();
        }

        public override Ability selectAbility()
        {
            throw new NotImplementedException();
        }
    }
}
