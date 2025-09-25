import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Eye } from "lucide-react";
import ViewCarDialogContent from "./ViewCarDialogContent";

type ViewCarDialogProps = {
  id: string;
};

const ViewCarDialog = ({ id }: ViewCarDialogProps) => {
  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Eye />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Car details</DialogTitle>
          <DialogDescription>In depth</DialogDescription>
        </DialogHeader>
        <ViewCarDialogContent id={id} className="grid gap-4" />
      </DialogContent>
    </Dialog>
  );
};

export default ViewCarDialog;
