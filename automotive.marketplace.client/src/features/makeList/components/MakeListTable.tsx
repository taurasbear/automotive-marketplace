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
import { useDeleteMake } from "../api/useDeleteMake";
import EditMakeDialog from "./EditMakeDialog";

type MakeListTableProps = {
  className?: string;
};

const MakeListTable = ({ className }: MakeListTableProps) => {
  const { data: makesQuery } = useQuery(getAllMakesOptions);
  const makes = makesQuery?.data || [];

  const { mutateAsync: deleteMakeAsync } = useDeleteMake();

  const handleDelete = async (id: string) => {
    await deleteMakeAsync({ id });
  };

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>A list of makes</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Created by</TableHead>
            <TableHead>Created at</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {makes.map((m) => (
            <TableRow key={m.id}>
              <TableCell>{m.name}</TableCell>
              <TableCell>{m.createdBy}</TableCell>
              <TableCell>{new Date(m.createdAt).toLocaleDateString()}</TableCell>
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
                      <AlertDialogTitle>Delete {m.name}?</AlertDialogTitle>
                      <AlertDialogDescription>
                        This action cannot be undone.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction onClick={() => handleDelete(m.id)}>
                        Delete
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
