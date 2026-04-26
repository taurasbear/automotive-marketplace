import { useQuery } from "@tanstack/react-query";
import {
  BarChart2,
  CheckCircle,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  Camera,
  FileText,
  Tag,
  Palette,
} from "lucide-react";
import { useState } from "react";
import { getSellerListingInsightsOptions } from "@/features/listingDetails";

type Props = {
  listingId: string;
};

export function SellerInsightsPanel({ listingId }: Props) {
  const [expanded, setExpanded] = useState(false);
  const { data, isLoading } = useQuery(
    getSellerListingInsightsOptions(listingId),
  );

  if (isLoading) {
    return (
      <div className="mt-2 space-y-2 rounded-md border p-3">
        <div className="bg-muted h-4 w-32 animate-pulse rounded" />
        <div className="bg-muted h-3 w-full animate-pulse rounded" />
      </div>
    );
  }

  if (!data) return null;
  const { marketPosition, listingQuality } = data.data;

  const priceColor =
    marketPosition.priceDifferencePercent == null
      ? "text-muted-foreground"
      : marketPosition.priceDifferencePercent >= 0
        ? "text-green-600"
        : "text-orange-500";

  const qualityColor =
    listingQuality.qualityScore >= 70
      ? "text-green-600"
      : listingQuality.qualityScore >= 50
        ? "text-blue-600"
        : "text-orange-500";

  return (
    <div className="border-border mt-2 rounded-md border">
      <button
        onClick={() => setExpanded(!expanded)}
        className="flex w-full items-center justify-between p-3 text-left"
      >
        <span className="flex items-center gap-2 text-sm font-medium">
          <BarChart2 className="h-4 w-4" />
          Seller Insights
        </span>
        {expanded ? (
          <ChevronUp className="h-4 w-4" />
        ) : (
          <ChevronDown className="h-4 w-4" />
        )}
      </button>

      {expanded && (
        <div className="grid grid-cols-2 gap-3 border-t p-3">
          {/* Market Position Card */}
          <div className="bg-muted/30 space-y-2 rounded-md p-3">
            <p className="text-xs font-semibold tracking-wide uppercase">
              Market Position
            </p>
            <div>
              <p className="text-muted-foreground text-xs">Your price</p>
              <p className="font-semibold">
                €{marketPosition.listingPrice.toLocaleString()}
              </p>
            </div>
            {marketPosition.hasMarketData ? (
              <>
                <div>
                  <p className="text-muted-foreground text-xs">Market median</p>
                  <p className="text-sm">
                    €{marketPosition.marketMedianPrice?.toLocaleString()}
                  </p>
                </div>
                <p className={`text-sm font-medium ${priceColor}`}>
                  {marketPosition.priceDifferencePercent != null &&
                    (marketPosition.priceDifferencePercent >= 0
                      ? `${marketPosition.priceDifferencePercent.toFixed(1)}% below market`
                      : `${Math.abs(marketPosition.priceDifferencePercent).toFixed(1)}% above market`)}
                </p>
                <p className="text-muted-foreground text-xs">
                  {marketPosition.marketListingCount} similar listings •{" "}
                  {marketPosition.daysListed} days listed
                </p>
              </>
            ) : (
              <p className="text-muted-foreground text-xs">
                No market data available yet
              </p>
            )}
          </div>

          {/* Listing Quality Card */}
          <div className="bg-muted/30 space-y-2 rounded-md p-3">
            <p className="text-xs font-semibold tracking-wide uppercase">
              Listing Quality
            </p>
            <div className={`text-2xl font-bold ${qualityColor}`}>
              {listingQuality.qualityScore}
              <span className="text-muted-foreground text-sm font-normal">
                /100
              </span>
            </div>
            <div className="space-y-1">
              {[
                {
                  check: listingQuality.hasDescription,
                  icon: FileText,
                  label: "Description",
                },
                {
                  check: listingQuality.hasPhotos,
                  icon: Camera,
                  label: `Photos (${listingQuality.photoCount})`,
                },
                { check: listingQuality.hasVin, icon: Tag, label: "VIN" },
                {
                  check: listingQuality.hasColour,
                  icon: Palette,
                  label: "Colour",
                },
              ].map(({ check, label }) => (
                <div key={label} className="flex items-center gap-1.5 text-xs">
                  {check ? (
                    <CheckCircle className="h-3.5 w-3.5 text-green-500" />
                  ) : (
                    <AlertTriangle className="h-3.5 w-3.5 text-orange-400" />
                  )}
                  <span className={check ? "" : "text-muted-foreground"}>
                    {label}
                  </span>
                </div>
              ))}
            </div>
            {listingQuality.suggestions.length > 0 && (
              <div className="space-y-1 border-t pt-2">
                {listingQuality.suggestions.map((s, i) => (
                  <p key={i} className="text-muted-foreground text-xs">
                    • {s}
                  </p>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
