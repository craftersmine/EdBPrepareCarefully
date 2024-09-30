using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully {
    public class PanelAgeRefactored : PanelModule {
        public static readonly int DaysPerYear = 60;

        public delegate void UpdateAgeHandler(int? ageYears, int? ageDays);

        public event UpdateAgeHandler BiologicalAgeUpdated;
        public event UpdateAgeHandler ChronologicalAgeUpdated;

        protected Rect RectDevelopmentStageButton;
        protected Rect RectBiologicalAgeLabel;
        protected Rect RectBiologicalAgeFieldYears;
        protected Rect RectBiologicalAgeFieldDays;
        protected Rect RectChronologicalAgeLabel;
        protected Rect RectChronologicalAgeFieldYears;
        protected Rect RectChronologicalAgeFieldDays;
        protected Rect RectYearsLabel;
        protected Rect RectDaysLabel;
        protected Rect RectDevelopmentalStageDropdown;
        protected Rect RectHeader;

        private WidgetNumberField biologicalFieldYears;
        private WidgetNumberField biologicalFieldDays;
        private WidgetNumberField chronologicalFieldYears;
        private WidgetNumberField chronologicalFieldDays;
        public static int AgeChronologicalMax = 3200;
        private AgeModifier ageModifier = new AgeModifier();

        public ModState State { get; set; }
        public ViewState ViewState { get; set; }
        public ProviderAgeLimits ProviderAgeLimits { get; set; }

        public PanelAgeRefactored() {
            biologicalFieldYears = new WidgetNumberField() {
                DragSlider = new DragSlider(0.4f, 20, 100),
                MinValue = 0,
                MaxValue = 99,
                UpdateAction = (int value) => {
                    UpdateBiologicalAgeYears(value);
                }
            };
            biologicalFieldDays = new WidgetNumberField() {
                DragSlider = new DragSlider(0.4f, 15, 100),
                MinValue = 0,
                MaxValue = 59,
                UpdateAction = (int value) => {
                    UpdateBiologicalAgeDays(value);
                }
            };
            chronologicalFieldYears = new WidgetNumberField() {
                DragSlider = new DragSlider(0.4f, 15, 100),
                MinValue = 0,
                MaxValue = AgeChronologicalMax,
                UpdateAction = (int value) => {
                    UpdateChronologicalAgeYears(value);
                }
            };
            chronologicalFieldDays = new WidgetNumberField() {
                DragSlider = new DragSlider(0.4f, 15, 100),
                MinValue = 0,
                MaxValue = 59,
                UpdateAction = (int value) => {
                    UpdateChronologicalAgeDays(value);
                }
            };
        }

        protected void UpdateBiologicalAgeYears(int? value) {
            BiologicalAgeUpdated?.Invoke(value, null);
        }
        protected void UpdateBiologicalAgeDays(int? value) {
            BiologicalAgeUpdated?.Invoke(null, value);
        }

        protected void UpdateChronologicalAgeYears(int? value) {
            ChronologicalAgeUpdated?.Invoke(value, null);
        }
        protected void UpdateChronologicalAgeDays(int? value) {
            ChronologicalAgeUpdated?.Invoke(null, value);
        }

        public override void Resize(float width) {
            base.Resize(width);

            float panelPadding = Style.SizePanelPadding.x;
            float availableSize = width - panelPadding * 2;

            GameFont saveFont = Text.Font;
            Text.Font = GameFont.Small;
            Vector2 bioLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Label.BiologicalAge".Translate());
            Vector2 chronoLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Label.ChronologicalAge".Translate());
            Text.Font = saveFont;
            float minimumLabelWidth = Mathf.Max(bioLabelSize.x, chronoLabelSize.x);

            float arrowPadding = 1;
            float arrowWidth = Textures.TextureButtonNext.width;
            float widthNeededForArrowButtons = 4 * (arrowWidth + arrowPadding);

            float paddingBetweenLabelAndField = 8;
            float paddingBetweenFields = 8;
            float fieldHeight = 22;

            float widthNeededForFixedElements = minimumLabelWidth + paddingBetweenFields + paddingBetweenLabelAndField + widthNeededForArrowButtons;
            float availableWidthForFields = availableSize - widthNeededForFixedElements;

            float yearFieldWidth = Mathf.Ceil(availableWidthForFields * 0.6f);
            float dayFieldWidth = availableWidthForFields - yearFieldWidth;

            float yearFieldOffset = panelPadding + minimumLabelWidth + paddingBetweenLabelAndField + arrowPadding + arrowWidth;
            float dayFieldOffset = yearFieldOffset + yearFieldWidth + paddingBetweenFields + arrowPadding * 2 + arrowWidth * 2;

            float developmentStageDropdownWidth = 30;
            RectDevelopmentalStageDropdown = new Rect(width - developmentStageDropdownWidth - panelPadding, 0, developmentStageDropdownWidth, 30);

            RectBiologicalAgeLabel = new Rect(panelPadding, 0, bioLabelSize.x, fieldHeight);
            RectBiologicalAgeFieldYears = new Rect(yearFieldOffset, RectBiologicalAgeLabel.yMin, yearFieldWidth, fieldHeight);
            RectBiologicalAgeFieldDays = new Rect(dayFieldOffset, RectBiologicalAgeLabel.yMin, dayFieldWidth, fieldHeight);

            RectChronologicalAgeLabel = new Rect(panelPadding, 0, chronoLabelSize.x, fieldHeight);
            RectChronologicalAgeFieldYears = new Rect(yearFieldOffset, RectChronologicalAgeLabel.yMin, yearFieldWidth, fieldHeight);
            RectChronologicalAgeFieldDays = new Rect(dayFieldOffset, RectBiologicalAgeLabel.yMin, dayFieldWidth, fieldHeight);

            saveFont = Text.Font;
            Text.Font = GameFont.Small;
            Vector2 yearsLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Label.Years".Translate());
            Vector2 daysLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Label.Days".Translate());
            Text.Font = saveFont;

            RectYearsLabel = new Rect(yearFieldOffset + yearFieldWidth * 0.5f - yearsLabelSize.x * 0.5f, 0, yearsLabelSize.x, yearsLabelSize.y);
            RectDaysLabel = new Rect(dayFieldOffset + dayFieldWidth * 0.5f - daysLabelSize.x * 0.5f, 0, daysLabelSize.x, daysLabelSize.y);
        }

        public override float Draw(float y) {

            // Update field values.
            CustomizedPawn customizedPawn = ViewState.CurrentPawn;
            Pawn pawn = customizedPawn.Pawn;
            int maxAge = ProviderAgeLimits.MaxAgeForPawn(pawn);
            int minAge = ProviderAgeLimits.MinAgeForPawn(pawn);
            chronologicalFieldYears.MinValue = minAge;
            biologicalFieldYears.MinValue = minAge;
            biologicalFieldYears.MaxValue = maxAge;

            float top = y;
            y += Margin.y;

            y += DrawHeader(y, Width, "EdB.PC.Panel.Age.Header".Translate().Resolve());
            Rect rect;

            Text.Font = GameFont.Tiny;
            GUI.color = Style.ColorText;
            Text.Anchor = TextAnchor.MiddleCenter;
            rect = RectYearsLabel.OffsetBy(0, y - RectYearsLabel.height);
            Widgets.Label(rect, "EdB.PC.Panel.Age.Label.Years".Translate());
            rect = RectDaysLabel.OffsetBy(0, y - RectDaysLabel.height);
            Widgets.Label(rect, "EdB.PC.Panel.Age.Label.Days".Translate());

            Text.Font = GameFont.Small;
            GUI.color = Style.ColorText;
            Text.Anchor = TextAnchor.MiddleLeft;
            rect = RectBiologicalAgeLabel.OffsetBy(0, y);
            Widgets.Label(rect, "EdB.PC.Panel.Age.Label.BiologicalAge".Translate());
            // Biological age years field.
            rect = RectBiologicalAgeFieldYears.OffsetBy(0, y);
            biologicalFieldYears.Draw(rect, pawn.ageTracker.AgeBiologicalYears);
            // Biological age days field.
            rect = RectBiologicalAgeFieldDays.OffsetBy(0, y);
            biologicalFieldDays.Draw(rect, AgeModifier.TicksToDays(pawn.ageTracker.AgeBiologicalTicks) % DaysPerYear);

            y += RectBiologicalAgeLabel.height;

            // Vertical padding between the two age field pairs
            y += 6;

            Text.Font = GameFont.Small;
            GUI.color = Style.ColorText;
            Text.Anchor = TextAnchor.MiddleLeft;
            rect = RectChronologicalAgeLabel.OffsetBy(0, y);
            Widgets.Label(rect, "EdB.PC.Panel.Age.Label.ChronologicalAge".Translate());
            // Chronological age years field.
            rect = RectChronologicalAgeFieldYears.OffsetBy(0, y);
            chronologicalFieldYears.Draw(rect, pawn.ageTracker.AgeChronologicalYears);
            // Chronological age days field.
            rect = RectChronologicalAgeFieldDays.OffsetBy(0, y);
            chronologicalFieldDays.Draw(rect, AgeModifier.TicksToDays(pawn.ageTracker.AgeChronologicalTicks) % DaysPerYear);

            y += RectChronologicalAgeLabel.height;

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            return y - top;
        }
    }
}
