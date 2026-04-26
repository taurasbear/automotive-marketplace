import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getListingComparisonAiSummaryOptions } from "../api/getListingComparisonAiSummaryOptions";

type Props = {
  listingAId: string;
  listingBId: string;
};

export function CompareAiSummary({ listingAId, listingBId }: Props) {
  const { data, isFetching, refetch } = useQuery(
    getListingComparisonAiSummaryOptions(listingAId, listingBId),
  );

  const summary = data?.data;
  const hasResult = summary?.isGenerated;

  return (
    <div className="border-border mb-4 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">AI Comparison Summary</span>
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
          {hasResult ? "Regenerate" : "Compare with AI"}
        </Button>
      </div>

      {isFetching && (
        <div className="mt-3 space-y-2">
          <div className="bg-muted h-3 w-full animate-pulse rounded" />
          <div className="bg-muted h-3 w-4/5 animate-pulse rounded" />
        </div>
      )}

      {!isFetching && hasResult && summary?.summary && (
        <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          Click "Compare with AI" to get a recommendation between these two listings.
        </p>
      )}
    </div>
  );
}
