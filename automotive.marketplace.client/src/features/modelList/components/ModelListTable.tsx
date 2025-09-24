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
import { getAllModelsOptions } from "../api/getAllModelsOptions";
import { useDeleteModel } from "../api/useDeleteModel";
import EditModelDialog from "./EditModelDialog";
import ViewModelDialog from "./ViewModelDialog";

type ModelListTableProps = {
  className?: string;
};

const ModelListTable = ({ className }: ModelListTableProps) => {
  const { data: modelsQuery } = useQuery(getAllModelsOptions);
  const models = modelsQuery?.data || [];

  const { mutateAsync: deleteModelAsync } = useDeleteModel();

  const handleDelete = async (id: string) => {
    await deleteModelAsync({ id });
  };

  return (
    <Table className={cn(className)}>
      <TableCaption>A list of models</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>First appearance</TableHead>
          <TableHead>Discontinued</TableHead>
          <TableHead>Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {models.map((m) => (
          <TableRow key={m.id}>
            <TableCell>{m.name}</TableCell>
            <TableCell>{m.firstAppearanceDate}</TableCell>
            <TableCell>{String(m.isDiscontinued)}</TableCell>
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
  );
};

export default ModelListTable;
