import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
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
import { getAllMakesOptions } from "@/api/make/getAllMakesOptions";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useDeleteMake } from "../api/useDeleteMake";
import EditMakeDialog from "./EditMakeDialog";

type MakeListTableProps = {
  className?: string;
};

const MakeListTable = ({ className }: MakeListTableProps) => {
  const { t } = useTranslation(["admin", "common"]);
  const { data: makesQuery } = useQuery(getAllMakesOptions);
  const makes = makesQuery?.data || [];

  const { mutateAsync: deleteMakeAsync } = useDeleteMake();

  const handleDelete = async (id: string) => {
    await deleteMakeAsync({ id });
  };

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>{t("admin:makes.tableDescription")}</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>{t("admin:makes.name")}</TableHead>
            <TableHead>{t("admin:makes.createdBy")}</TableHead>
            <TableHead>{t("admin:makes.createdAt")}</TableHead>
            <TableHead>{t("admin:makes.actions")}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {makes.map((m) => (
            <TableRow key={m.id}>
              <TableCell>{m.name}</TableCell>
              <TableCell>{m.createdBy}</TableCell>
              <TableCell>
                {new Date(m.createdAt).toLocaleDateString()}
              </TableCell>
              <TableCell className="flex gap-1">
                <EditMakeDialog id={m.id} />
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button variant="secondary">
                      <Trash />
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>
                        {t("admin:makes.deleteConfirm", { name: m.name })}
                      </AlertDialogTitle>
                      <AlertDialogDescription>
                        {t("admin:makes.deleteWarning")}
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>
                        {t("common:actions.cancel")}
                      </AlertDialogCancel>
                      <AlertDialogAction onClick={() => handleDelete(m.id)}>
                        {t("common:actions.delete")}
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default MakeListTable;
