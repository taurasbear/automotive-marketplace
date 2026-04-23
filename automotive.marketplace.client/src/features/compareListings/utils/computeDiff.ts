import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap, DiffResult } from "../types/diff";

const HIGHER_IS_BETTER = new Set<keyof GetListingByIdResponse>([
  "powerKw",
  "year",
]);
const LOWER_IS_BETTER = new Set<keyof GetListingByIdResponse>([
  "mileage",
  "price",
]);

export function computeDiff(
  a: GetListingByIdResponse,
  b: GetListingByIdResponse,
): DiffMap {
  const result: DiffMap = {};

  for (const field of HIGHER_IS_BETTER) {
    const aVal = a[field] as number;
    const bVal = b[field] as number;
    result[field] =
      aVal === bVal ? "equal" : aVal > bVal ? "a-better" : "b-better";
  }

  for (const field of LOWER_IS_BETTER) {
    const aVal = a[field] as number;
    const bVal = b[field] as number;
    result[field] =
      aVal === bVal ? "equal" : aVal < bVal ? "a-better" : "b-better";
  }

  const allFields = Object.keys(a) as (keyof GetListingByIdResponse)[];
  for (const field of allFields) {
    if (field in result) continue;
    const aVal = a[field];
    const bVal = b[field];
    if (Array.isArray(aVal) || Array.isArray(bVal)) continue;
    const diff: DiffResult =
      String(aVal) === String(bVal) ? "equal" : "different";
    result[field] = diff;
  }

  return result;
}
