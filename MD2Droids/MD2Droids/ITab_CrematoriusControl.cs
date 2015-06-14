using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MD2
{
    public class ITab_CrematoriusControl : ITab
    {
        protected static Vector2 WindowSize;
        protected static Vector2 ItemSize;
        protected static Vector2 ButtonSize = new Vector2(100f, 40f);
        protected static Vector2 PriorityButtonSize = new Vector2(25f, 25f);
        protected const float itemMargin = 2f;
        protected const float buttonMargin = 1f;
        protected const float borderMargin = 20f;

        protected static readonly Color offButtonColor = new Color(0.65f, 0.65f, 0.65f, 0.5f);
        protected static readonly Color cremateButtonColor = new Color(1f, 0.5f, 0.25f, 0.5f);
        protected static readonly Color butcherButtonColor = new Color(1f, 0.35f, 0.35f, 0.5f);

        protected static readonly Texture2D offButtonTexture = SolidColorMaterials.NewSolidColorTexture(offButtonColor);
        protected static readonly Texture2D cremateButtonTexture = SolidColorMaterials.NewSolidColorTexture(cremateButtonColor);
        protected static readonly Texture2D butcherButtonTexture = SolidColorMaterials.NewSolidColorTexture(butcherButtonColor);

        protected float currentY = 0f;

        public ITab_CrematoriusControl()
        {
            float x, y;
            int numberOfButtons = 4;
            x = (ButtonSize.x * numberOfButtons) + (buttonMargin * (numberOfButtons - 1)) + borderMargin * 2 + PriorityButtonSize.x;
            ItemSize = new Vector2(x, ButtonSize.y);
            x = ItemSize.x + (2 * borderMargin);
            y = (ItemSize.y * 3) + (itemMargin * 2) + (2 * borderMargin);
            WindowSize = new Vector2(x, y);
            this.size = WindowSize;
            this.labelKey = "TabCrematorius";
        }

        protected override void FillTab()
        {
            Rect mainRect = new Rect(0, 0, size.x, size.y);
            Rect useRect = mainRect.ContractedBy(borderMargin);
            currentY = 0f;
            List<CrematoriusTarget> targets = ((Crematorius)SelPawn).GetTargets.OrderBy((CrematoriusTarget t) => t.NaturalPriority).ThenBy((CrematoriusTarget t) => t.Label).ToList();

            try
            {
                GUI.BeginGroup(useRect);
                foreach (var target in targets)
                {
                    DrawItemRow(target, currentY);
                    currentY += ItemSize.y + itemMargin;
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawItemRow(CrematoriusTarget target, float y)
        {
            Rect itemRect = new Rect(0, y, ItemSize.x, ItemSize.y);

            try
            {
                GUI.BeginGroup(itemRect);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                float currentX = 0f;

                Rect labelRect = new Rect(currentX, 0, ButtonSize.x, ButtonSize.y);
                Widgets.Label(labelRect, target.Label);
                currentX += ButtonSize.x + buttonMargin;

                //Off selection button
                Rect offButtonRect = new Rect(currentX, 0, ButtonSize.x, ButtonSize.y);
                GUI.DrawTexture(offButtonRect, offButtonTexture);
                if (offButtonRect.Contains(Event.current.mousePosition))
                {
                    Widgets.DrawHighlight(offButtonRect);
                }
                if (target.Mode == CrematoriusOperationMode.Off)
                {
                    Widgets.DrawBox(offButtonRect);
                }
                Widgets.Label(offButtonRect, "Off".Translate());
                if (Widgets.InvisibleButton(offButtonRect))
                {
                    target.Mode = CrematoriusOperationMode.Off;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                currentX += ButtonSize.x + buttonMargin;

                //Cremate selection button
                Rect cremateButtonRect = new Rect(currentX, 0, ButtonSize.x, ButtonSize.y);
                GUI.DrawTexture(cremateButtonRect, cremateButtonTexture);
                if (cremateButtonRect.Contains(Event.current.mousePosition))
                {
                    Widgets.DrawHighlight(cremateButtonRect);
                }
                if (target.Mode == CrematoriusOperationMode.Cremate)
                {
                    Widgets.DrawBox(cremateButtonRect);
                }
                Widgets.Label(cremateButtonRect, "Cremate".Translate());
                if (Widgets.InvisibleButton(cremateButtonRect))
                {
                    target.Mode = CrematoriusOperationMode.Cremate;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                currentX += ButtonSize.x + buttonMargin;

                //Butcher selection button
                Rect butcherButtonRect = new Rect(currentX, 0f, ButtonSize.x, ButtonSize.y);
                GUI.DrawTexture(butcherButtonRect, butcherButtonTexture);
                if (butcherButtonRect.Contains(Event.current.mousePosition))
                {
                    Widgets.DrawHighlight(butcherButtonRect);
                }
                if (target.Mode == CrematoriusOperationMode.Butcher)
                {
                    Widgets.DrawBox(butcherButtonRect);
                }
                Widgets.Label(butcherButtonRect, "Butcher".Translate());
                if (Widgets.InvisibleButton(butcherButtonRect))
                {
                    target.Mode = CrematoriusOperationMode.Butcher;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                currentX += ButtonSize.x + buttonMargin;

                //Do the rotten only check box
                //Rect onlyRottenRect = new Rect(currentX, 0, ButtonSize.x, ButtonSize.y);
                //Widgets.LabelCheckbox(onlyRottenRect, "Only rotten".Translate(), ref target.OnlyRotten);
                //if (onlyRottenRect.Contains(Event.current.mousePosition))
                //{
                //    Widgets.DrawHighlight(onlyRottenRect);
                //}
                //currentX += ButtonSize.x + buttonMargin;

                //Do the priority changer
                currentX += borderMargin;
                float y2 = (ItemSize.y - PriorityButtonSize.y) / 2f;
                Rect priorityRect = new Rect(currentX, y2, PriorityButtonSize.x, PriorityButtonSize.y);
                GUI.DrawTexture(priorityRect, offButtonTexture);
                if (priorityRect.Contains(Event.current.mousePosition))
                {
                    Widgets.DrawHighlight(priorityRect);
                }
                Widgets.DrawBox(priorityRect);
                Text.Font = GameFont.Medium;
                GUI.color = ColorOfPriority(target.Priority);
                Widgets.Label(priorityRect, target.Priority.ToString());
                TooltipHandler.TipRegion(priorityRect, new TipSignal("CrematoriusPriorityTooltip".Translate()));
                if (priorityRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        //Decrease the priority
                        target.Priority--;
                        SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                    }
                    if (Event.current.button == 1)
                    {
                        //Increase the priority
                        target.Priority++;
                        SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                    }
                    Event.current.Use();
                }
            }
            finally
            {
                GUI.EndGroup();
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        public static Color ColorOfPriority(int prio)
        {
            switch (prio)
            {
                case 1:
                    return Color.green;
                case 2:
                    return new Color(1f, 0.9f, 0.6f);
                case 3:
                    return new Color(0.8f, 0.7f, 0.5f);
                case 4:
                    return new Color(0.6f, 0.6f, 0.6f);
                default:
                    return Color.grey;
            }
        }
    }
}
