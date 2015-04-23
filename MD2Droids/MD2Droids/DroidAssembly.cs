using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace MD2
{
    public class DroidAssembly : Building_WorkTable
    {
        List<ResearchRecipeDef> recipeList;
        List<string> recipesAdded = new List<string>();
        public override void SpawnSetup()
        {
            string name = this.def.defName;
            recipeList = DefDatabase<ResearchRecipeDef>.AllDefsListForReading.Where(r => r.buildingDefName == name).ToList();
            this.def.recipes = new List<RecipeDef>();
            base.SpawnSetup();
            CheckResearch();
        }

        public override void TickRare()
        {
            base.TickRare();
            CheckResearch();
        }

        private void CheckResearch()
        {
            if (Game.GodMode)
            {
                foreach (ResearchRecipeDef c in recipeList)
                {
                    if (!this.recipesAdded.Contains(c.recipeDef.defName))
                    {
                        addRecipeToRecipes(c.recipeDef);
                    }
                }
            }
            else
            {
                foreach (ResearchRecipeDef current in recipeList)
                {
                    if (current.researchDef != null)
                    {
                        if (Find.ResearchManager.IsFinished(current.researchDef) && !this.recipesAdded.Contains(current.recipeDef.defName))
                        {
                            addRecipeToRecipes(current.recipeDef);
                        }
                    }
                    else
                        addRecipeToRecipes(current.recipeDef);
                }
            }
        }

        private void addRecipeToRecipes(RecipeDef def)
        {
            if (this.def.recipes != null)
            {
                if (!this.recipesAdded.Contains(def.defName))
                {
                    this.recipesAdded.Add(def.defName);
                    this.def.recipes.Add(def);
                    typeof(ThingDef).GetField("allRecipesCached", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this.def, null);
                }
            }
            else
                Log.Message("recipes list was null " + this.def.defName);


        }
    }
}
