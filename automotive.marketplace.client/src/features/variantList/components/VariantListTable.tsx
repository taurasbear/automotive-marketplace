import { getVariantsByModelIdOptions } from "@/features/variantList/api/getVariantsByModelIdOptions";
import { Button } from "@/components/ui/button";
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
import { Trash } from "lucide-react";
import { useDeleteVariant } from "../api/useDeleteVariant";
import EditVariantDialog from "./EditVariantDialog";
import ViewVariantDialog from "./ViewVariantDialog";

type VariantListTableProps = {
  modelId: string;
  makeId: string;
  className?: string;
};

const VariantListTable = ({
  modelId,
  makeId,
  className,
}: VariantListTableProps) => {
  const { data: variantsQuery, isLoading, isError } = useQuery(
    getVariantsByModelIdOptions(modelId),
  );

  const { mutateAsync: deleteVariantAsync } = useDeleteVariant();

  const handleDelete = async (id: string) => {
    await deleteVariantAsync({ id });
  };

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
            <TableHead>Actions</TableHead>
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
              <TableCell>
                <ViewVariantDialog variant={v} />
                <EditVariantDialog variant={v} makeId={makeId} />
                <Button
                  variant="secondary"
                  onClick={() => handleDelete(v.id)}
                >
                  <Trash />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default VariantListTable;
