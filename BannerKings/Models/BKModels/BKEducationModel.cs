using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKEducationModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }

        public ExplainedNumber CalculateLessonsCost(Hero student, Hero instructor)
        {
            var result = new ExplainedNumber(1000f, true);

            return result;
        }

        public ExplainedNumber CalculateLanguageLimit(Hero learner)
        {
            var result = new ExplainedNumber(2f, true);

            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipAvidLearner))
            {
                result.Add(1f, BKPerks.Instance.ScholarshipAvidLearner.Name);
            }

            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipBookWorm))
            {
                result.Add(1f, BKPerks.Instance.ScholarshipBookWorm.Name);
            }

            if (learner.GetPerkValue(BKPerks.Instance.ScholarshipPolyglot))
            {
                result.Add(2f, BKPerks.Instance.ScholarshipPolyglot.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateLanguageLearningRate(Hero student, Hero instructor, Language language)
        {
            var result = new ExplainedNumber(1f, true);
            result.LimitMin(0f);
            result.LimitMax(5f);

            result.Add(student.GetSkillValue(BKSkills.Instance.Scholarship) * 0.01f, BKSkills.Instance.Scholarship.Name);

            if (instructor != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(instructor);
                var teaching = data.GetLanguageFluency(language) - 1f;
                if (!float.IsNaN(teaching) && teaching != 0f)
                {
                    result.AddFactor(teaching, new TextObject("{=nNESSaBb}Instructor fluency"));
                }
            }
            else
            {
                return new ExplainedNumber(0f);
            }


            var native = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(student);
            var dic = native.Inteligible;
            if (dic.ContainsKey(language))
            {
                result.Add(dic[language], new TextObject("{=k5G2c550}Intelligibility with {LANGUAGE}").SetTextVariable("LANGUAGE", native.Name));
            }

            if (student.GetPerkValue(BKPerks.Instance.ScholarshipAvidLearner))
            {
                result.AddFactor(0.2f, BKPerks.Instance.ScholarshipAvidLearner.Name);
            }

            var studentData = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(student);
            var overLimit = studentData.Languages.Count - CalculateLanguageLimit(student).ResultNumber;
            if (overLimit > 0f)
            {
                result.AddFactor(-0.33f * overLimit, new TextObject("{=1ssSRbe5}Over languages limit"));
            }

            return result;
        }

        public ExplainedNumber CalculateBookReadingRate(BookType book, Hero reader)
        {
            var result = new ExplainedNumber(0f, true);
            var fluency = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(reader).GetLanguageFluency(book.Language);
            result.Add(fluency, new TextObject("{=vRMD0fdw}{LANGUAGE} fluency").SetTextVariable("LANGUAGE", book.Language.Name));

            var books = BannerKingsConfig.Instance.EducationManager.GetAvailableBooks(reader.PartyBelongedTo);
            if (books.Contains(DefaultBookTypes.Instance.Dictionary))
            {
                result.Add(0.2f, DefaultBookTypes.Instance.Dictionary.Name);
            }

            if (reader.GetPerkValue(BKPerks.Instance.ScholarshipWellRead))
            {
                result.AddFactor(0.12f, BKPerks.Instance.ScholarshipWellRead.Name);
            }

            if (reader.GetPerkValue(BKPerks.Instance.ScholarshipBookWorm))
            {
                result.AddFactor(0.20f, BKPerks.Instance.ScholarshipBookWorm.Name);
            }

            {
                var intell = reader.GetAttributeValue(TaleWorlds.Core.DefaultCharacterAttributes.Intelligence);
                var mod = MathF.Pow(1.05, intell) - 1;
                result.AddFactor((float)mod, TO_Intelligence);
            }

            return result;
        }

        private static readonly TextObject TO_Intelligence = new TextObject("Intelligence");
    }
}