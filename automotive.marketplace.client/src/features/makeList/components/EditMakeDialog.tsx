import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Pencil } from "lucide-react";
import { Suspense, useState } from "react";
import { useUpdateMake } from "../api/useUpdateMake";
import { MakeFormData } from "../types/MakeFormData";
import EditMakeDialogContent from "./EditMakeDialogContent";

type EditMakeDialogProps = {
  id: string;
};

const EditMakeDialog = ({ id }: EditMakeDialogProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const { mutateAsync: updateMakeAsync } = useUpdateMake();

  const handleSubmit = async (formData: MakeFormData) => {
    await updateMakeAsync({ ...formData, id });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <Suspense fallback={<p className="text-sm text-muted-foreground">Loading…</p>}>
          <EditMakeDialogContent id={id} onSubmit={handleSubmit} />
        </Suspense>
      </DialogContent>
    </Dialog>
  );
};

export default EditMakeDialog;
