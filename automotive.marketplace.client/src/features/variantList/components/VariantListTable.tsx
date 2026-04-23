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
import { useTranslation } from "react-i18next";
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
  const { t } = useTranslation("admin");
  const {
    data: variantsQuery,
    isLoading,
    isError,
  } = useQuery(getVariantsByModelIdOptions(modelId));

  const { mutateAsync: deleteVariantAsync } = useDeleteVariant();

  const handleDelete = async (id: string) => {
    await deleteVariantAsync({ id });
  };

  if (isLoading) return <p>{t("variants.loading")}</p>;
  if (isError) return <p>{t("variants.failed")}</p>;

  const variants = variantsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>{t("variants.tableDescription")}</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>{t("variants.fuel")}</TableHead>
            <TableHead>{t("variants.transmission_col")}</TableHead>
            <TableHead>{t("variants.bodyType_col")}</TableHead>
            <TableHead>{t("variants.doors_col")}</TableHead>
            <TableHead>{t("variants.powerKw_col")}</TableHead>
            <TableHead>{t("variants.engineMl_col")}</TableHead>
            <TableHead>{t("variants.custom")}</TableHead>
            <TableHead>{t("variants.actions")}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {variants.map((v) => (
            <TableRow key={v.id}>
              <TableCell>{v.fuelName}</TableCell>
              <TableCell>{v.transmissionName}</TableCell>
              <TableCell>{v.bodyTypeName}</TableCell>
              <TableCell>{v.doorCount}</TableCell>
              <TableCell>{v.powerKw}</TableCell>
              <TableCell>{v.engineSizeMl}</TableCell>
              <TableCell>
                {v.isCustom ? t("variants.yes") : t("variants.no")}
              </TableCell>
              <TableCell>
                <ViewVariantDialog variant={v} />
                <EditVariantDialog variant={v} makeId={makeId} />
                <Button variant="secondary" onClick={() => handleDelete(v.id)}>
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
