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
  const { data: variantsQuery, isPending, isError } = useQuery(
    getVariantsByModelIdOptions(modelId || undefined),
  );

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
          <TableHead>Fuel</TableHead>
          <TableHead>Transmission</TableHead>
          <TableHead>Power (kW)</TableHead>
          <TableHead>Engine (ml)</TableHead>
          <TableHead>Body Type</TableHead>
          <TableHead>Doors</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {isPending && (
          <TableRow>
            <TableCell colSpan={6} className="text-center text-sm text-muted-foreground">
              Loading variants…
            </TableCell>
          </TableRow>
        )}
        {isError && (
          <TableRow>
            <TableCell colSpan={6} className="text-center text-sm text-destructive">
              Failed to load variants
            </TableCell>
          </TableRow>
        )}
        {!isPending && !isError && variants.length === 0 && (
          <TableRow>
            <TableCell colSpan={6} className="text-center text-sm text-muted-foreground">
              No variants available for this model
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
