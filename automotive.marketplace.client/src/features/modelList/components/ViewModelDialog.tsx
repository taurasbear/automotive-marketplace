import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { Eye } from "lucide-react";
import { getModelByIdOptions } from "../api/getModelByIdOptions";

type ViewModelDialogProps = {
  id: string;
};

const ViewModelDialog = ({ id }: ViewModelDialogProps) => {
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));

  const model = modelQuery.data;

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Eye />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Model details</DialogTitle>
          <DialogDescription>In depth</DialogDescription>
        </DialogHeader>
        <div className="grid gap-4">
          <h3>{model.name}</h3>
          <p>First appeared: {model.firstAppearanceDate}</p>
          {model.isDiscontinued ? (
            <p>Has been discontinued</p>
          ) : (
            <p>Has not been discontinued</p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default ViewModelDialog;
