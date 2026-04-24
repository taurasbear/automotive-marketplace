import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import { useTranslation } from "react-i18next";
import { formatNumber } from "@/lib/i18n/formatNumber";
import type { DiffMap } from "../types/diff";
import { CompareRow } from "./CompareRow";

type RowSpec = {
  field: keyof GetListingByIdResponse;
  label: string;
  format?: (
    value: GetListingByIdResponse[keyof GetListingByIdResponse],
  ) => string;
};

type SectionSpec = {
  label: string;
  rows: RowSpec[];
};

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
}: CompareTableProps) => {
  const { t } = useTranslation("compare");

  const TABLE_SECTIONS: SectionSpec[] = [
    {
      label: t("table.basicInfo"),
      rows: [
        { field: "makeName", label: t("table.make") },
        { field: "modelName", label: t("table.model") },
        { field: "bodyTypeName", label: t("table.bodyType") },
        { field: "year", label: t("table.year") },
        {
          field: "isUsed",
          label: t("table.condition"),
          format: (v) => (v ? t("table.used") : t("table.new")),
        },
        {
          field: "mileage",
          label: t("table.mileage"),
          format: (v) => `${formatNumber(v as number)} km`,
        },
        { field: "city", label: t("table.city") },
      ],
    },
    {
      label: t("table.engineAndPerformance"),
      rows: [
        { field: "powerKw", label: t("table.powerKw") },
        { field: "engineSizeMl", label: t("table.engineSizeMl") },
        { field: "fuelName", label: t("table.fuelType") },
        { field: "transmissionName", label: t("table.transmission") },
        { field: "drivetrainName", label: t("table.drivetrain") },
      ],
    },
    {
      label: t("table.listingDetails"),
      rows: [
        {
          field: "price",
          label: t("table.price"),
          format: (v) => `${(v as number).toFixed(0)} €`,
        },
        { field: "status", label: t("table.status") },
        { field: "sellerName", label: t("table.seller") },
      ],
    },
  ];

  return (
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
};
