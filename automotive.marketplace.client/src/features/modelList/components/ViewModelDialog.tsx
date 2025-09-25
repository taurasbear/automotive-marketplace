import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Eye } from "lucide-react";
import ViewModelDialogContent from "./ViewModelDialogContent";

type ViewModelDialogProps = {
  id: string;
};

const ViewModelDialog = ({ id }: ViewModelDialogProps) => {
  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Eye />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <ViewModelDialogContent id={id} />
      </DialogContent>
    </Dialog>
  );
};

export default ViewModelDialog;
