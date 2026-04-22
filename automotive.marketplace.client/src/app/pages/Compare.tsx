import {
  getListingComparisonOptions,
  CompareHeader,
  CompareSearchModal,
  CompareTable,
  DiffToggleFab,
  computeDiff,
} from "@/features/compareListings";
import { router } from "@/lib/router";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Route } from "@/app/routes/compare";

const Compare = () => {
  const { a, b } = Route.useSearch();
  const [diffOnly, setDiffOnly] = useState(false);
  const [swapSlot, setSwapSlot] = useState<"a" | "b" | null>(null);

  const {
    data: response,
    isLoading,
    isError,
  } = useQuery(getListingComparisonOptions(a, b));

  if (isLoading) {
    return (
      <div className="py-8">
        <div className="bg-muted mb-4 h-48 animate-pulse rounded-lg" />
        <div className="space-y-2">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="bg-muted h-10 animate-pulse rounded" />
          ))}
        </div>
      </div>
    );
  }

  if (isError || !response) {
    return (
      <div className="py-16 text-center">
        <p className="text-muted-foreground">
          One or more listings could not be found.
        </p>
      </div>
    );
  }

  const { listingA, listingB } = response.data;
  const diffMap = computeDiff(listingA, listingB);

  return (
    <div className="py-8">
      <CompareHeader
        listingA={listingA}
        listingB={listingB}
        onChangeA={() => setSwapSlot("a")}
        onChangeB={() => setSwapSlot("b")}
      />
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={diffMap}
        diffOnly={diffOnly}
      />
      <DiffToggleFab
        active={diffOnly}
        onToggle={() => setDiffOnly((d) => !d)}
      />
      <CompareSearchModal
        open={swapSlot !== null}
        onClose={() => setSwapSlot(null)}
        excludeIds={swapSlot === "a" ? [b] : [a]}
        onSelect={(id) => {
          const slot = swapSlot;
          setSwapSlot(null);
          void router.navigate({
            to: "/compare",
            search: slot === "a" ? { a: id, b } : { a, b: id },
          });
        }}
      />
    </div>
  );
};

export default Compare;
