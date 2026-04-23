import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getVariantsByModelIdOptions } from "@/features/variantList/api/getVariantsByModelIdOptions";
import { Variant } from "@/features/variantList/types/Variant";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";

type VariantTableProps = {
  modelId: string;
  selectedVariantId: string;
  onSelect: (variant: Variant | null) => void;
  disabled?: boolean;
};

const VariantTable = ({
  modelId,
  selectedVariantId,
  onSelect,
  disabled,
}: VariantTableProps) => {
  const { t } = useTranslation("common");
  const {
    data: variantsQuery,
    isPending,
    isError,
  } = useQuery(getVariantsByModelIdOptions(modelId || undefined));

  if (!modelId || disabled) return null;

  const variants = variantsQuery?.data ?? [];

  const handleRowClick = (variant: Variant) => {
    if (variant.id === selectedVariantId) {
      onSelect(null);
    } else {
      onSelect(variant);
    }
  };

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>{t("variantTable.fuel")}</TableHead>
          <TableHead>{t("variantTable.transmission")}</TableHead>
          <TableHead>{t("variantTable.powerKw")}</TableHead>
          <TableHead>{t("variantTable.engineMl")}</TableHead>
          <TableHead>{t("variantTable.bodyType")}</TableHead>
          <TableHead>{t("variantTable.doors")}</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {isPending && (
          <TableRow>
            <TableCell
              colSpan={6}
              className="text-muted-foreground text-center text-sm"
            >
              {t("variantTable.loading")}
            </TableCell>
          </TableRow>
        )}
        {isError && (
          <TableRow>
            <TableCell
              colSpan={6}
              className="text-destructive text-center text-sm"
            >
              {t("variantTable.failed")}
            </TableCell>
          </TableRow>
        )}
        {!isPending && !isError && variants.length === 0 && (
          <TableRow>
            <TableCell
              colSpan={6}
              className="text-muted-foreground text-center text-sm"
            >
              {t("variantTable.noVariants")}
            </TableCell>
          </TableRow>
        )}
        {variants.map((v) => (
          <TableRow
            key={v.id}
            onClick={() => handleRowClick(v)}
            className={`cursor-pointer ${v.id === selectedVariantId ? "bg-primary/10 font-medium" : "hover:bg-muted/50"}`}
          >
            <TableCell>{v.fuelName}</TableCell>
            <TableCell>{v.transmissionName}</TableCell>
            <TableCell>{v.powerKw}</TableCell>
            <TableCell>{v.engineSizeMl}</TableCell>
            <TableCell>{v.bodyTypeName}</TableCell>
            <TableCell>{v.doorCount}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
};

export default VariantTable;
