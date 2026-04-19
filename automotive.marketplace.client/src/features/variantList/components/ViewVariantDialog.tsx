import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Eye } from "lucide-react";
import { Variant } from "../types/Variant";
import ViewVariantDialogContent from "./ViewVariantDialogContent";

type ViewVariantDialogProps = {
  variant: Variant;
};

const ViewVariantDialog = ({ variant }: ViewVariantDialogProps) => {
  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Eye />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <ViewVariantDialogContent variant={variant} />
      </DialogContent>
    </Dialog>
  );
};

export default ViewVariantDialog;
