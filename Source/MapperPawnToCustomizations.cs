using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully {
    public class MapperPawnToCustomizations {
        public ProviderHealthOptions ProviderHealth { get; set; }
        public ProviderAlienRaces ProviderAlienRaces { get; set; }

        public CustomizationsPawn Map(Pawn pawn) {
            var result = new CustomizationsPawn();

            MapKindDef(pawn, result);
            MapMutantDef(pawn, result);
            MapXenotype(pawn, result);
            MapGenes(pawn, result);
            MapGender(pawn, result);
            MapName(pawn, result);
            MapAge(pawn, result);
            MapBackstories(pawn, result);
            MapFavoriteColor(pawn, result);
            MapTraits(pawn, result);
            MapAppearance(pawn, result);
            MapTitles(pawn, result);
            MapAlienRace(pawn, result);
            MapIdeo(pawn, result);
            MapSkills(pawn, result);
            MapApparel(pawn, result);
            MapAbilities(pawn, result);
            MapInjuriesAndImplants(pawn, result);
            MapOtherValues(pawn, result);

            return result;
        }

        public void MapGenes(Pawn pawn, CustomizationsPawn customizations) {
            if (ModsConfig.BiotechActive) {
                customizations.Genes = new CustomizedGenes() {
                    Endogenes = CreateCustomizedGeneList(pawn, pawn.genes.Endogenes),
                    Xenogenes = CreateCustomizedGeneList(pawn, pawn.genes.Xenogenes)
                };
            }
            else {
                customizations.Genes = null;
            }
        }

        private List<CustomizedGene> CreateCustomizedGeneList(Pawn pawn, IEnumerable<Gene> genes) {
            var result = new List<CustomizedGene>();
            if (genes == null) {
                return result;
            }
            foreach (var gene in genes) {
                GeneDef overriddenByEndogene = null;
                GeneDef overriddenByXenogene = null;
                if (gene.overriddenByGene != null) {
                    if (pawn?.genes?.Endogenes.Contains(gene.overriddenByGene) ?? false) {
                        overriddenByEndogene = gene.overriddenByGene.def;
                    }
                    else if (pawn?.genes?.Xenogenes.Contains(gene.overriddenByGene) ?? false) {
                        overriddenByXenogene = gene.overriddenByGene.def;
                    }
                }
                result.Add(new CustomizedGene() {
                    GeneDef = gene.def,
                    OverriddenByEndogene = overriddenByEndogene,
                    OverriddenByXenogene = overriddenByXenogene,
                });
                //Logger.Debug("  Mapped gene " + gene.def.defName + ", overridden by = " + gene.overriddenByGene?.def?.defName);
            }
            return result;
        }

        public void MapIdeo(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Ideo = pawn.ideo?.Ideo;
            customizations.Certainty = pawn.ideo?.Certainty ?? 0.0f;
        }

        public void MapKindDef(Pawn pawn, CustomizationsPawn customizations) {
            customizations.PawnKind = pawn.kindDef;
        }
        public void MapMutantDef(Pawn pawn, CustomizationsPawn customizations) {
            if (pawn.mutant != null) {
                customizations.Mutant = new CustomizedMutant() {
                    MutantDef = pawn.mutant.Def
                };
            }
            else {
                customizations.Mutant = null;
            }
        }

        public void MapXenotype(Pawn pawn, CustomizationsPawn customizations) {
            customizations.XenotypeDef = pawn.genes.Xenotype;
            customizations.XenotypeName = pawn.genes.xenotypeName;
            customizations.UniqueXenotype = pawn.genes.UniqueXenotype;
            customizations.CustomXenotype = pawn.genes.CustomXenotype;
        }
        public void MapGender(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Gender = pawn.gender;
        }
        public void MapName(Pawn pawn, CustomizationsPawn customizations) {
            if (typeof(NameTriple).IsAssignableFrom(pawn.Name.GetType())) {
                NameTriple nameTriple = pawn.Name as NameTriple;
                customizations.NameType = "Triple";
                customizations.FirstName = nameTriple.First;
                customizations.NickName = nameTriple.Nick;
                customizations.LastName = nameTriple.Last;
                customizations.SingleName = null;
            }
            else if (typeof(NameSingle).IsAssignableFrom(pawn.Name.GetType())) {
                customizations.NameType = "Single";
                customizations.SingleName = (pawn.Name as NameSingle).Name;
                customizations.FirstName = null;
                customizations.NickName = null;
                customizations.LastName = null;
            }
            else {
                customizations.NameType = "Triple";
                customizations.FirstName = null;
                customizations.NickName = null;
                customizations.LastName = null;
                customizations.SingleName = null;
            }
        }
        public void MapAge(Pawn pawn, CustomizationsPawn customizations) {
            customizations.BiologicalAgeInTicks = pawn.ageTracker.AgeBiologicalTicks;
            customizations.ChronologicalAgeInTicks = pawn.ageTracker.AgeChronologicalTicks;
        }
        public void MapBackstories(Pawn pawn, CustomizationsPawn customizations) {
            customizations.AdulthoodBackstory = pawn.story.GetBackstory(BackstorySlot.Adulthood);
            customizations.ChildhoodBackstory = pawn.story.GetBackstory(BackstorySlot.Childhood);
        }
        public void MapTraits(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Traits = new List<CustomizationsTrait>();
            foreach (var trait in pawn.story.traits.allTraits) {
                customizations.Traits.Add(new CustomizationsTrait() {
                    TraitDef = trait.def,
                    Degree = trait.Degree
                });
            }
        }
        public void MapAppearance(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Hair = pawn.story.hairDef;
            customizations.HairColor = pawn.story.HairColor;
            customizations.HeadType = pawn.story.headType;
            customizations.BodyType = pawn.story.bodyType;
            customizations.Beard = pawn.style.beardDef;
            customizations.BodyTattoo = pawn.style.BodyTattoo;
            customizations.FaceTattoo = pawn.style.FaceTattoo;
            customizations.SkinColor = pawn.story.SkinColor;
            customizations.SkinColorOverride = pawn.story.skinColorOverride;
        }

        public void MapSkills(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Skills.Clear();
            foreach (var skill in pawn.skills.skills) {
                var skillCustomization = new CustomizationsSkill() {
                    SkillDef = skill.def,
                    Passion = skill.passion,
                    OriginalPassion = skill.passion,
                    Level = skill.Level,
                    OriginalLevel = skill.Level
                };
                customizations.Skills.Add(skillCustomization);
            }
        }

        public void MapStartingPossessions(IEnumerable<ThingDefCount> possessions, CustomizationsPawn customizations) {
            customizations.Possessions.Clear();
            foreach (ThingDefCount p in possessions) {
                customizations.Possessions.Add(new CustomizedPossession() { ThingDef = p.ThingDef, Count = p.Count });
            }
        }

        public void MapApparel(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Apparel.Clear();
            foreach (var apparel in pawn.apparel.WornApparel) {
                var customizedApparel = new CustomizationsApparel() {
                    ThingDef = apparel.def,
                    StyleCategoryDef = apparel.StyleDef?.Category,
                    StuffDef = apparel.Stuff,
                    Quality = apparel.GetQuality()
                };
                if (apparel.TryGetQuality(out QualityCategory qualityCategory)) {
                    customizedApparel.Quality = qualityCategory;
                }
                if (apparel.def.useHitPoints) {
                    customizedApparel.HitPoints = UtilityApparel.HitPointPercentForApparel(apparel);
                }
                else {
                    //Logger.Debug("Apparel doesn't use hitpoints");
                }
                if (apparel.def.HasComp<CompColorable>()) {
                    CompColorable colorable = apparel.TryGetComp<CompColorable>();
                    if (colorable != null && colorable.Active) {
                        customizedApparel.Color = colorable.Color;
                    }
                }
                //Logger.Debug(string.Format("Mapping apparel {0}, {1}, {2}, {3}", customizedApparel.ThingDef?.defName, customizedApparel.StuffDef?.defName, customizedApparel.Quality, customizedApparel.HitPoints));
                customizations.Apparel.Add(customizedApparel);
            }
        }
        public void MapAbilities(Pawn pawn, CustomizationsPawn customizations) {
            customizations.Abilities.Clear();
            foreach (var ability in pawn.abilities.AllAbilitiesForReading) {
                customizations.Abilities.Add(new CustomizedAbility() {
                    AbilityDef = ability.def
                });
            }
        }

        public void MapInjuriesAndImplants(Pawn pawn, CustomizationsPawn customizations) {
            OptionsHealth healthOptions = ProviderHealth.GetOptions(pawn);
            List<Injury> injuries = new List<Injury>();
            List<Implant> implants = new List<Implant>();

            // Create a lookup of all of the body parts that are missing
            HashSet<BodyPartRecord> missingParts = new HashSet<BodyPartRecord>();
            foreach (var hediff in pawn.health.hediffSet.hediffs) {
                if (hediff is Hediff_MissingPart || hediff is Hediff_AddedPart) {
                    missingParts.Add(hediff.Part);
                }
            }

            foreach (var hediff in pawn.health.hediffSet.hediffs) {
                InjuryOption option = healthOptions.FindInjuryOptionByHediffDef(hediff.def);
                if (option != null) {
                    //Logger.Debug("Found injury option for {" + hediff.def.defName + "} for part {" + hediff.Part?.LabelCap + "}");

                    // If the hediff is a missing part and the part's parent is also missing, we don't add a missing part hediff for the child part.
                    if (hediff is Hediff_MissingPart) {
                        if (hediff.Part.parent != null && missingParts.Contains(hediff.Part.parent)) {
                            continue;
                        }
                    }

                    Injury injury = new Injury();
                    injury.BodyPartRecord = hediff.Part;
                    injury.Option = option;
                    if (hediff is Hediff_Level hediffLevel) {
                        injury.Severity = hediffLevel.level;
                    }
                    else {
                        injury.Severity = hediff.Severity;
                    }
                    injury.Hediff = hediff;
                    HediffComp_GetsPermanent getsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
                    if (getsPermanent != null) {
                        injury.PainFactor = getsPermanent.PainFactor;
                    }

                    injury.Chemical = (hediff as Hediff_ChemicalDependency)?.chemical;

                    injuries.Add(injury);
                }
                else {
                    //Logger.Debug("Did not find injury option for {" + hediff.def.defName + "} for part {" + hediff.Part?.LabelCap + "}");
                    ImplantOption implantOption = healthOptions.FindImplantOptionsThatAddHediff(hediff).RandomElementWithFallback(null);
                    HediffDef hediffDef = hediff?.def;
                    BodyPartRecord bodyPartRecord = hediff.Part;
                    if (hediffDef != null) {
                        var implant = new Implant() {
                            Option = implantOption,
                            Recipe = implantOption?.RecipeDef,
                            BodyPartRecord = bodyPartRecord,
                            Hediff = hediff,
                            HediffDef = hediffDef,
                        };
                        if (hediff is Hediff_Level level) {
                            //Logger.Debug("Mapping implant " + implantOption.HediffDef.defName + " with severity " + level.level);
                            if (level.level >= 0) {
                                implant.Severity = level.level;
                            }
                        }
                        implants.Add(implant);
                    }
                    else {
                        Logger.Debug("Could not find implant option for hediff {" + hediff.def?.defName + "} and body part {" + (hediff.Part?.def?.defName ?? "WholeBody") + "}");
                    }
                }
            }
            customizations.Injuries.Clear();
            customizations.Implants.Clear();
            foreach (var injury in injuries) {
                //Logger.Debug("Adding injury: " + injury.Option.Label);
                customizations.Injuries.Add(injury);
            }
            foreach (var implant in implants) {
                //Logger.Debug("Adding implant: " + implant.Label);
                customizations.Implants.Add(implant);
            }
        }

        public void MapTitles(Pawn pawn, CustomizationsPawn customizations) {
            List<CustomizationTitle> titleList = new List<CustomizationTitle>();
            foreach (var faction in Find.World.factionManager.AllFactionsInViewOrder) {
                int favor = pawn.royalty.GetFavor(faction);
                var title = pawn.royalty.GetCurrentTitleInFaction(faction);
                if (favor == 0 && title == null) {
                    continue;
                }
                CustomizationTitle customizedTitle = new CustomizationTitle() {
                    Faction = title?.faction,
                    TitleDef = title?.def,
                    Honor = favor
                };
                titleList.Add(customizedTitle);
            }
            customizations.Titles = titleList;
        }

        public void MapAlienRace(Pawn pawn, CustomizationsPawn customizations) {
            customizations.AlienRace = ProviderAlienRaces.GetAlienRaceForPawn(pawn);
        }

        public void MapFavoriteColor(Pawn pawn, CustomizationsPawn customizations) {
            customizations.FavoriteColor = pawn.story.favoriteColor;
        }

        public void MapOtherValues(Pawn pawn, CustomizationsPawn customizations) {

        }
    }
}
