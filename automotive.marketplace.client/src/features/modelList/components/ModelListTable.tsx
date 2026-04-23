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
import { useDateLocale } from "@/lib/i18n/dateLocale";
import { format } from "date-fns";
import { getAllModelsOptions } from "../api/getAllModelsOptions";
import { useDeleteModel } from "../api/useDeleteModel";
import EditModelDialog from "./EditModelDialog";
import ViewModelDialog from "./ViewModelDialog";

type ModelListTableProps = {
  className?: string;
};

const ModelListTable = ({ className }: ModelListTableProps) => {
  const { t } = useTranslation("admin");
  const locale = useDateLocale();
  const { data: modelsQuery } = useQuery(getAllModelsOptions);
  const models = modelsQuery?.data || [];

  const { mutateAsync: deleteModelAsync } = useDeleteModel();

  const handleDelete = async (id: string) => {
    await deleteModelAsync({ id });
  };

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>{t("models.tableDescription")}</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>{t("models.name")}</TableHead>
            <TableHead>{t("models.createdBy")}</TableHead>
            <TableHead>{t("models.createdAt")}</TableHead>
            <TableHead>{t("models.actions")}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {models.map((m) => (
            <TableRow key={m.id}>
              <TableCell>{m.name}</TableCell>
              <TableCell>{m.createdBy}</TableCell>
              <TableCell>
                {format(new Date(m.createdAt), "P", { locale })}
              </TableCell>
              <TableCell>
                <ViewModelDialog id={m.id} />
                <EditModelDialog id={m.id} />
                <Button variant="secondary" onClick={() => handleDelete(m.id)}>
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

export default ModelListTable;
