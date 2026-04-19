import { getVariantsByModelIdOptions } from "@/features/variantList/api/getVariantsByModelIdOptions";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";

type VariantListTableProps = {
  modelId: string;
  className?: string;
};

const VariantListTable = ({ modelId, className }: VariantListTableProps) => {
  const { data: variantsQuery, isLoading, isError } = useQuery(
    getVariantsByModelIdOptions(modelId),
  );

  if (isLoading) return <p>Loading variants…</p>;
  if (isError) return <p>Failed to load variants.</p>;

  const variants = variantsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>A list of variants</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>Year</TableHead>
            <TableHead>Fuel</TableHead>
            <TableHead>Transmission</TableHead>
            <TableHead>Body type</TableHead>
            <TableHead>Doors</TableHead>
            <TableHead>Power (kW)</TableHead>
            <TableHead>Engine (ml)</TableHead>
            <TableHead>Custom</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {variants.map((v) => (
            <TableRow key={v.id}>
              <TableCell>{v.year}</TableCell>
              <TableCell>{v.fuelName}</TableCell>
              <TableCell>{v.transmissionName}</TableCell>
              <TableCell>{v.bodyTypeName}</TableCell>
              <TableCell>{v.doorCount}</TableCell>
              <TableCell>{v.powerKw}</TableCell>
              <TableCell>{v.engineSizeMl}</TableCell>
              <TableCell>{v.isCustom ? "Yes" : "No"}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default VariantListTable;
