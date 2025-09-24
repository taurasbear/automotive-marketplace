import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { getModelByIdOptions } from "../api/getModelByIdOptions";
import { useUpdateModel } from "../api/useUpdateModel";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

type ViewModelDialogProps = {
  id: string;
};

const EditModelDialog = ({ id }: ViewModelDialogProps) => {
  const [isEditModelDialogOpen, setIsEditModelDialogOpen] = useState<boolean>();

  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));
  const model = modelQuery.data;

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
        <DialogHeader>
          <DialogTitle>Edit {model.name}</DialogTitle>
        </DialogHeader>
        <ModelForm
          model={{
            ...model,
            firstAppearanceDate: new Date(model.firstAppearanceDate),
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default EditModelDialog;
