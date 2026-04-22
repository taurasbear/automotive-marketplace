import { getListingComparisonOptions } from "@/features/compareListings/api/getListingComparisonOptions";
import {
  CompareHeader,
  CompareTable,
  DiffToggleFab,
} from "@/features/compareListings";
import { computeDiff } from "@/features/compareListings/utils/computeDiff";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Route } from "@/app/routes/compare";

const Compare = () => {
  const { a, b } = Route.useSearch();
  const [diffOnly, setDiffOnly] = useState(false);

  const { data: response, isLoading, isError } = useQuery(
    getListingComparisonOptions(a, b),
  );

  if (isLoading) {
    return (
      <div className="py-8">
        <div className="mb-4 h-48 animate-pulse rounded-lg bg-muted" />
        <div className="space-y-2">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="h-10 animate-pulse rounded bg-muted" />
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
      <CompareHeader listingA={listingA} listingB={listingB} />
      <CompareTable
        listingA={listingA}
        listingB={listingB}
        diffMap={diffMap}
        diffOnly={diffOnly}
      />
      <DiffToggleFab active={diffOnly} onToggle={() => setDiffOnly((d) => !d)} />
    </div>
  );
};

export default Compare;
