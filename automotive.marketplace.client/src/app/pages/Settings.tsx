import { useQuery } from "@tanstack/react-query";
import { BarChart2, SlidersHorizontal, Sparkles } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  getUserPreferencesOptions,
  QuizModal,
  useUpsertUserPreferences,
} from "@/features/userPreferences";

export default function Settings() {
  const { t } = useTranslation("userPreferences");
  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const { mutateAsync: upsert } = useUpsertUserPreferences();
  const [quizOpen, setQuizOpen] = useState(false);

  const prefs = prefsData?.data;

  const handleScoringToggle = async (checked: boolean) => {
    if (!prefs) return;
    await upsert({
      valueWeight: prefs.valueWeight,
      efficiencyWeight: prefs.efficiencyWeight,
      reliabilityWeight: prefs.reliabilityWeight,
      mileageWeight: prefs.mileageWeight,
      conditionWeight: prefs.conditionWeight,
      autoGenerateAiSummary: prefs.autoGenerateAiSummary,
      enableVehicleScoring: checked,
    });
  };

  const handleAiSummaryToggle = async (checked: boolean) => {
    if (!prefs) return;
    await upsert({
      valueWeight: prefs.valueWeight,
      efficiencyWeight: prefs.efficiencyWeight,
      reliabilityWeight: prefs.reliabilityWeight,
      mileageWeight: prefs.mileageWeight,
      conditionWeight: prefs.conditionWeight,
      autoGenerateAiSummary: checked,
      enableVehicleScoring: prefs.enableVehicleScoring,
    });
  };

  const scoringEnabled = prefs?.enableVehicleScoring ?? false;

  return (
    <div className="container mx-auto max-w-2xl py-8">
      <h1 className="mb-6 text-2xl font-bold">{t("settings.title")}</h1>
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">
            {t("settings.listingPreferences")}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-0">
          <div className="flex items-center justify-between py-4">
            <div className="flex items-center gap-3">
              <BarChart2 className="text-muted-foreground h-5 w-5" />
              <div>
                <p className="text-sm font-medium">
                  {t("settings.scoringLabel")}
                </p>
                <p className="text-muted-foreground text-xs">
                  {t("settings.scoringDescription")}
                </p>
              </div>
            </div>
            <Switch
              checked={scoringEnabled}
              onCheckedChange={handleScoringToggle}
            />
          </div>

          <div
            className={`flex items-center justify-between py-3 pl-8 ${!scoringEnabled ? "opacity-50" : ""}`}
          >
            <div className="flex items-center gap-3">
              <SlidersHorizontal className="text-muted-foreground h-4 w-4" />
              <p className="text-sm font-medium">
                {t("settings.scoringPreferencesLabel")}
              </p>
            </div>
            <Button
              variant="outline"
              size="sm"
              disabled={!scoringEnabled}
              onClick={() => setQuizOpen(true)}
            >
              {t("settings.scoringPreferencesButton")}
            </Button>
          </div>

          <Separator />

          <div className="flex items-center justify-between py-4">
            <div className="flex items-center gap-3">
              <Sparkles className="text-muted-foreground h-5 w-5" />
              <div>
                <p className="text-sm font-medium">
                  {t("settings.aiSummaryLabel")}
                </p>
                <p className="text-muted-foreground text-xs">
                  {t("settings.aiSummaryDescription")}
                </p>
              </div>
            </div>
            <Switch
              checked={prefs?.autoGenerateAiSummary ?? false}
              onCheckedChange={handleAiSummaryToggle}
            />
          </div>
        </CardContent>
      </Card>

      <QuizModal
        open={quizOpen}
        onOpenChange={setQuizOpen}
        initialWeights={
          prefs?.hasPreferences
            ? {
                valueWeight: prefs.valueWeight,
                efficiencyWeight: prefs.efficiencyWeight,
                reliabilityWeight: prefs.reliabilityWeight,
                mileageWeight: prefs.mileageWeight,
                conditionWeight: prefs.conditionWeight,
              }
            : undefined
        }
        initialStep={prefs?.hasPreferences ? 2 : undefined}
      />
    </div>
  );
}
