import { useQuery } from "@tanstack/react-query";
import {
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  SlidersHorizontal,
  Info,
} from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAppSelector } from "@/hooks/redux";
import {
  QuizModal,
  getUserPreferencesOptions,
} from "@/features/userPreferences";
import { getListingScoreOptions } from "../api/getListingScoreOptions";
import type { ScoreFactor } from "../types/GetListingScoreResponse";

type ScoreCardProps = {
  listingId: string;
};

const factorTranslationKeys: Record<string, string> = {
  "Market Value": "score.value",
  Efficiency: "score.efficiency",
  Reliability: "score.reliability",
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function FactorBar({
  label,
  factor,
  secondaryText,
  t,
}: {
  label: string;
  factor: ScoreFactor;
  secondaryText?: string;
  t: (key: string, opts?: Record<string, unknown>) => string;
}) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-between py-1 text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="text-muted-foreground flex items-center gap-1">
          <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
          {t("score.noData")}
        </span>
      </div>
    );
  }
  return (
    <div className="py-1">
      <div className="mb-1 flex justify-between text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-medium">{Math.round(factor.score)}</span>
      </div>
      <div className="bg-muted h-1.5 w-full rounded-full">
        <div
          className={`h-1.5 rounded-full ${factor.score >= 70 ? "bg-green-500" : factor.score >= 50 ? "bg-blue-500" : "bg-orange-500"}`}
          style={{ width: `${Math.round(factor.score)}%` }}
        />
      </div>
      {secondaryText && (
        <p className="text-muted-foreground mt-0.5 text-xs">{secondaryText}</p>
      )}
    </div>
  );
}

export function ScoreCard({ listingId }: ScoreCardProps) {
  const { t } = useTranslation("userPreferences");
  const [expanded, setExpanded] = useState(false);
  const [quizOpen, setQuizOpen] = useState(false);
  const { userId } = useAppSelector((state) => state.auth);
  const { data: prefsData } = useQuery(getUserPreferencesOptions);
  const isAuthenticated = !!userId;
  const prefs = prefsData?.data;
  const scoringEnabled = prefs?.enableVehicleScoring ?? false;

  const { data, isLoading } = useQuery({
    ...getListingScoreOptions(listingId),
    enabled: scoringEnabled,
  });

  if (!scoringEnabled) {
    return (
      <div className="border-border rounded-lg border p-4">
        <p className="text-muted-foreground text-sm">
          {t("score.enableScoring")}
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="border-border rounded-lg border p-4">
        <div className="flex items-center gap-4">
          <div className="bg-muted h-[72px] w-[72px] animate-pulse rounded-full" />
          <div className="flex-1 space-y-2">
            <div className="bg-muted h-4 w-32 animate-pulse rounded" />
            <div className="bg-muted h-3 w-24 animate-pulse rounded" />
          </div>
        </div>
      </div>
    );
  }

  if (!data) return null;

  const score = data.data;
  const scoredCount = [
    score.value,
    score.efficiency,
    score.reliability,
    score.mileage,
    score.condition,
  ].filter((f) => f.status === "scored").length;

  const defectCount =
    score.condition.status === "scored"
      ? Math.round((100 - score.condition.score) / 20)
      : 0;
  const conditionSecondary =
    defectCount > 0
      ? t("score.defects", { count: defectCount })
      : t("score.defectsNone");

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center gap-4">
        <div className="flex h-[72px] w-[72px] flex-shrink-0 flex-col items-center justify-center rounded-full border-2 border-current">
          <span
            className={`text-2xl leading-none font-bold ${scoreColor(score.overallScore)}`}
          >
            {score.overallScore}
          </span>
          <span
            className={`text-xs font-medium ${scoreColor(score.overallScore)}`}
          >
            /100
          </span>
        </div>
        <div className="flex-1">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{t("score.title")}</p>
              <p className="text-muted-foreground text-xs">
                {score.isPersonalized
                  ? t("score.personalized")
                  : t("score.unPersonalized")}
              </p>
            </div>
            <button
              onClick={() => setExpanded(!expanded)}
              className="text-muted-foreground hover:text-foreground"
              aria-label={
                expanded ? "Collapse score breakdown" : "Expand score breakdown"
              }
            >
              {expanded ? (
                <ChevronUp className="h-4 w-4" />
              ) : (
                <ChevronDown className="h-4 w-4" />
              )}
            </button>
            {isAuthenticated && (
              <button
                onClick={() => setQuizOpen(true)}
                className="text-muted-foreground hover:text-foreground ml-2"
                aria-label="Personalize score weights"
              >
                <SlidersHorizontal className="h-4 w-4" />
              </button>
            )}
          </div>
          {score.hasMissingFactors && !expanded && (
            <p className="text-muted-foreground mt-1 flex items-center gap-1 text-xs">
              <AlertTriangle className="h-3 w-3 text-orange-400" />
              {t("score.missingFactors", {
                factors: score.missingFactors
                  .map((f) => t(factorTranslationKeys[f] ?? f))
                  .join(", "),
              })}
            </p>
          )}
        </div>
      </div>

      {expanded && (
        <div className="mt-4 space-y-1 border-t pt-3">
          <FactorBar label={t("score.value")} factor={score.value} t={t} />
          <FactorBar
            label={t("score.efficiency")}
            factor={score.efficiency}
            t={t}
          />
          <FactorBar
            label={t("score.reliability")}
            factor={score.reliability}
            t={t}
          />
          <FactorBar label={t("score.mileage")} factor={score.mileage} t={t} />
          <FactorBar
            label={t("score.condition")}
            factor={score.condition}
            secondaryText={conditionSecondary}
            t={t}
          />
          {scoredCount < 3 && (
            <p className="text-muted-foreground mt-2 flex items-center gap-1 text-xs">
              <Info className="h-3 w-3" />
              {t("score.limitedNotice")}
            </p>
          )}
        </div>
      )}
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
