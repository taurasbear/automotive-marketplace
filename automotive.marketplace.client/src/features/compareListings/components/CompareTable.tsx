import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import type { DiffMap } from "../types/diff";
import { CompareRow } from "./CompareRow";

type RowSpec = {
  field: keyof GetListingByIdResponse;
  label: string;
  format?: (value: GetListingByIdResponse[keyof GetListingByIdResponse]) => string;
};

type SectionSpec = {
  label: string;
  rows: RowSpec[];
};

const TABLE_SECTIONS: SectionSpec[] = [
  {
    label: "Basic Info",
    rows: [
      { field: "makeName", label: "Make" },
      { field: "modelName", label: "Model" },
      { field: "bodyTypeName", label: "Body Type" },
      { field: "year", label: "Year" },
      { field: "isUsed", label: "Condition", format: (v) => (v ? "Used" : "New") },
      {
        field: "mileage",
        label: "Mileage",
        format: (v) => `${(v as number).toLocaleString()} km`,
      },
      { field: "city", label: "City" },
    ],
  },
  {
    label: "Engine & Performance",
    rows: [
      { field: "powerKw", label: "Power (kW)" },
      { field: "engineSizeMl", label: "Engine Size (ml)" },
      { field: "fuelName", label: "Fuel Type" },
      { field: "transmissionName", label: "Transmission" },
      { field: "drivetrainName", label: "Drivetrain" },
    ],
  },
  {
    label: "Listing Details",
    rows: [
      {
        field: "price",
        label: "Price",
        format: (v) => `${(v as number).toFixed(0)} €`,
      },
      { field: "status", label: "Status" },
      { field: "sellerName", label: "Seller" },
    ],
  },
];

type CompareTableProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
  diffMap: DiffMap;
  diffOnly: boolean;
};

export const CompareTable = ({
  listingA,
  listingB,
  diffMap,
  diffOnly,
}: CompareTableProps) => (
  <div className="mt-4 overflow-x-auto">
    <table className="w-full border-collapse">
      <tbody>
        {TABLE_SECTIONS.flatMap((section) => {
          const visibleRows = diffOnly
            ? section.rows.filter((r) => diffMap[r.field] !== "equal")
            : section.rows;
          if (visibleRows.length === 0) return [];
          return [
            <tr key={`section-${section.label}`} className="bg-muted">
              <td colSpan={3} className="px-4 py-2 text-sm font-semibold">
                {section.label}
              </td>
            </tr>,
            ...visibleRows.map((row) => {
              const valA = listingA[row.field];
              const valB = listingB[row.field];
              const displayA = row.format
                ? row.format(valA)
                : String(valA ?? "—");
              const displayB = row.format
                ? row.format(valB)
                : String(valB ?? "—");
              return (
                <CompareRow
                  key={row.field}
                  label={row.label}
                  valueA={displayA}
                  valueB={displayB}
                  diff={diffMap[row.field] ?? "equal"}
                />
              );
            }),
          ];
        })}
      </tbody>
    </table>
  </div>
);
