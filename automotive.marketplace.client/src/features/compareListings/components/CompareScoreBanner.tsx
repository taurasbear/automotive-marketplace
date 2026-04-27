import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, SlidersHorizontal } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppSelector } from "@/hooks/redux";
import {
  UserPreferencesDialog,
  getUserPreferencesOptions,
} from "@/features/userPreferences";
import { getListingScoreOptions } from "@/features/listingDetails";
import type {
  GetListingScoreResponse,
  ScoreFactor,
} from "@/features/listingDetails";

type CompareScoreBannerProps = {
  listingAId: string;
  listingBId: string;
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function MiniFactorRow({ factor }: { factor: ScoreFactor }) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-center gap-1 text-xs">
        <AlertTriangle className="h-3 w-3 text-orange-400" />
        <span className="text-muted-foreground">—</span>
      </div>
    );
  }
  return (
    <div className="text-center text-xs font-medium">
      {Math.round(factor.score)}
    </div>
  );
}

function ScoreColumn({
  score,
  loading,
  t,
}: {
  score: GetListingScoreResponse | undefined;
  loading: boolean;
  t: (key: string) => string;
}) {
  if (loading) {
    return (
      <div className="flex flex-col items-center gap-2">
        <div className="bg-muted h-16 w-16 animate-pulse rounded-full" />
        <div className="bg-muted h-3 w-20 animate-pulse rounded" />
      </div>
    );
  }
  if (!score)
    return (
      <div className="text-muted-foreground text-sm">{t("score.noData")}</div>
    );

  return (
    <div className="flex flex-col items-center gap-2">
      <div
        className={`flex h-16 w-16 flex-col items-center justify-center rounded-full border-2 border-current ${scoreColor(score.overallScore)}`}
      >
        <span className="text-xl leading-none font-bold">
          {score.overallScore}
        </span>
        <span className="text-xs">/100</span>
      </div>
      <p className="text-muted-foreground text-xs">
        {score.isPersonalized
          ? t("score.personalized")
          : t("score.unPersonalized")}
      </p>
      <div className="w-full space-y-0.5 text-center">
        <MiniFactorRow factor={score.value} />
        <MiniFactorRow factor={score.efficiency} />
        <MiniFactorRow factor={score.reliability} />
        <MiniFactorRow factor={score.mileage} />
        <MiniFactorRow factor={score.condition} />
      </div>
    </div>
  );
}

export function CompareScoreBanner({
  listingAId,
  listingBId,
}: CompareScoreBannerProps) {
  const { t } = useTranslation("userPreferences");
  const [quizOpen, setQuizOpen] = useState(false);
  const { userId } = useAppSelector((state) => state.auth);
  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const isAuthenticated = !!userId;
  const prefs = prefsData?.data;
  const scoringEnabled = prefs?.enableVehicleScoring ?? false;

  const { data: aData, isLoading: aLoading } = useQuery({
    ...getListingScoreOptions(listingAId),
    enabled: scoringEnabled,
  });
  const { data: bData, isLoading: bLoading } = useQuery({
    ...getListingScoreOptions(listingBId),
    enabled: scoringEnabled,
  });

  if (!scoringEnabled) {
    return (
      <div className="bg-card border-border mb-4 rounded-lg border p-4">
        <p className="text-muted-foreground text-sm">
          {t("score.enableScoring")}
        </p>
      </div>
    );
  }

  return (
    <div className="bg-card border-border mb-4 rounded-lg border p-4">
      {isAuthenticated && (
        <div className="mb-3 flex items-center justify-between">
          <span className="text-muted-foreground text-sm">
            {t("score.title")}
          </span>
          <button
            onClick={() => setQuizOpen(true)}
            className="text-muted-foreground hover:text-foreground flex items-center gap-1 text-xs"
          >
            <SlidersHorizontal className="h-3.5 w-3.5" />
            {t("score.customize")}
          </button>
        </div>
      )}
      <div className="grid grid-cols-3 items-start gap-4">
        <div className="flex flex-col items-center gap-2 pt-16 text-xs">
          <div className="text-muted-foreground">{t("score.value")}</div>
          <div className="text-muted-foreground">{t("score.efficiency")}</div>
          <div className="text-muted-foreground">{t("score.reliability")}</div>
          <div className="text-muted-foreground">{t("score.mileage")}</div>
          <div className="text-muted-foreground">{t("score.condition")}</div>
        </div>
        <ScoreColumn score={aData?.data} loading={aLoading} t={t} />
        <ScoreColumn score={bData?.data} loading={bLoading} t={t} />
      </div>
      <UserPreferencesDialog open={quizOpen} onOpenChange={setQuizOpen} />
    </div>
  );
}
