import { useQuery } from "@tanstack/react-query";
import { Sparkles, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getListingAiSummaryOptions } from "../api/getListingAiSummaryOptions";

type Props = {
  listingId: string;
};

export function AiSummarySection({ listingId }: Props) {
  const {
    data,
    isFetching,
    refetch,
  } = useQuery(getListingAiSummaryOptions(listingId));

  const summary = data?.data;
  const hasResult = summary?.isGenerated;

  return (
    <div className="border-border rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Sparkles className="text-primary h-4 w-4" />
          <span className="text-sm font-semibold">AI Buyer Verdict</span>
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
          {hasResult ? "Regenerate" : "Generate"}
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
        <p className="text-muted-foreground mt-3 text-sm leading-relaxed">{summary.summary}</p>
      )}

      {!isFetching && data && !hasResult && (
        <p className="text-muted-foreground mt-3 text-sm italic">
          AI summary unavailable at this time.
        </p>
      )}

      {!isFetching && !hasResult && !data && (
        <p className="text-muted-foreground mt-3 text-sm">
          Click "Generate" to get an AI-powered buyer verdict for this listing.
        </p>
      )}
    </div>
  );
}
