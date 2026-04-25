import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { getListingScoreOptions } from "../api/getListingScoreOptions";
import type { ScoreFactor } from "../types/GetListingScoreResponse";

type ScoreCardProps = {
  listingId: string;
};

const FACTOR_LABELS: Record<string, string> = {
  value: "Market Value",
  efficiency: "Efficiency",
  reliability: "Reliability",
  mileage: "Mileage",
};

function scoreColor(score: number): string {
  if (score >= 70) return "text-green-600";
  if (score >= 50) return "text-blue-600";
  return "text-orange-500";
}

function FactorBar({ label, factor }: { label: string; factor: ScoreFactor }) {
  if (factor.status === "missing") {
    return (
      <div className="flex items-center justify-between py-1 text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="text-muted-foreground flex items-center gap-1">
          <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
          No data
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
    </div>
  );
}

export function ScoreCard({ listingId }: ScoreCardProps) {
  const [expanded, setExpanded] = useState(false);
  const { data, isLoading } = useQuery(getListingScoreOptions(listingId));

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

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center gap-4">
        <div className="flex h-[72px] w-[72px] flex-shrink-0 flex-col items-center justify-center rounded-full border-2 border-current">
          <span className={`text-2xl font-bold leading-none ${scoreColor(score.overallScore)}`}>
            {score.overallScore}
          </span>
          <span className={`text-xs font-medium ${scoreColor(score.overallScore)}`}>/100</span>
        </div>
        <div className="flex-1">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-semibold">Vehicle Score</p>
              <p className="text-muted-foreground text-xs">Un-personalized</p>
            </div>
            <button
              onClick={() => setExpanded(!expanded)}
              className="text-muted-foreground hover:text-foreground"
              aria-label={expanded ? "Collapse score breakdown" : "Expand score breakdown"}
            >
              {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </button>
          </div>
          {score.hasMissingFactors && !expanded && (
            <p className="text-muted-foreground mt-1 flex items-center gap-1 text-xs">
              <AlertTriangle className="h-3 w-3 text-orange-400" />
              Missing: {score.missingFactors.join(", ")}
            </p>
          )}
        </div>
      </div>

      {expanded && (
        <div className="mt-4 space-y-1 border-t pt-3">
          <FactorBar label={FACTOR_LABELS.value} factor={score.value} />
          <FactorBar label={FACTOR_LABELS.efficiency} factor={score.efficiency} />
          <FactorBar label={FACTOR_LABELS.reliability} factor={score.reliability} />
          <FactorBar label={FACTOR_LABELS.mileage} factor={score.mileage} />
        </div>
      )}
    </div>
  );
}
