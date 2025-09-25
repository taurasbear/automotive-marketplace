import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { format } from "date-fns";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useUpdateModel } from "../api/useUpdateModel";
import { ModelFormData } from "../types/ModelFormData";
import EditModelDialogContent from "./EditModelDialogContent";

type ViewModelDialogProps = {
  id: string;
};

const EditModelDialog = ({ id }: ViewModelDialogProps) => {
  const [isEditModelDialogOpen, setIsEditModelDialogOpen] =
    useState<boolean>(false);

  const { mutateAsync: updateModelAsync } = useUpdateModel();

  const handleSubmit = async (formData: ModelFormData) => {
    await updateModelAsync({
      ...formData,
      id,
      firstAppearanceDate: format(formData.firstAppearanceDate, "yyyy-MM-dd"),
    });

    setIsEditModelDialogOpen(false);
  };

  return (
    <Dialog
      open={isEditModelDialogOpen}
      onOpenChange={setIsEditModelDialogOpen}
    >
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <EditModelDialogContent id={id} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default EditModelDialog;
