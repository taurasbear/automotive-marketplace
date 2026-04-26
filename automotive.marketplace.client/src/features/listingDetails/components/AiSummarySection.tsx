import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw, Info } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { getListingAiSummaryOptions } from "../api/getListingAiSummaryOptions";

type Props = {
  listingId: string;
};

const factorTranslationKeys: Record<string, string> = {
  MarketValue: "score.value",
  Efficiency: "score.efficiency",
  Reliability: "score.reliability",
};

export function AiSummarySection({ listingId }: Props) {
  const { t, i18n } = useTranslation("listings");
  const { t: tPrefs } = useTranslation("userPreferences");

  const { data, isFetching, refetch } = useQuery(
    getListingAiSummaryOptions(listingId, i18n.language),
  );

  const summary = data?.data;
  const hasResult = summary?.isGenerated;
  const unavailable = summary?.unavailableFactors ?? [];
  const translatedUnavailable = unavailable.map((f) =>
    tPrefs(factorTranslationKeys[f] ?? f),
  );

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">{t("aiSummary.title")}</span>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => refetch()}
          disabled={isFetching}
          className="flex items-center gap-1"
        >
          {isFetching ? (
            <RefreshCw className="h-3.5 w-3.5 animate-spin" />
          ) : (
            <Sparkles className="h-3.5 w-3.5" />
          )}
          {hasResult ? t("aiSummary.regenerate") : t("aiSummary.generate")}
        </Button>
      </div>

      {isFetching && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
          <div className="bg-muted h-3 w-3/5 animate-pulse rounded" />
        </div>
      )}

      {!isFetching && hasResult && summary?.summary && (
        <>
          <p className="text-muted-foreground mt-3 text-sm leading-relaxed">
            {summary.summary}
          </p>
          {unavailable.length > 0 && (
            <Alert variant="default" className="mt-3">
              <Info className="h-4 w-4" />
              <AlertDescription className="text-xs">
                {t("aiSummary.unavailableFactors", {
                  factors: translatedUnavailable.join(", "),
                })}
              </AlertDescription>
            </Alert>
          )}
        </>
      )}

      {!isFetching && data && !hasResult && (
        <p className="text-muted-foreground mt-3 text-sm italic">
          {t("aiSummary.unavailable")}
        </p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          {t("aiSummary.prompt")}
        </p>
      )}
    </div>
  );
}
