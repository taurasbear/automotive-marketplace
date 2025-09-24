import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { format } from "date-fns";
import { useState } from "react";
import { useCreateModel } from "../api/useCreateModel";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

const CreateModelDialog = () => {
  const [isCreateModelDialogOpen, setIsCreateModelDialogOpen] =
    useState<boolean>();

  const { mutateAsync: createModelAsync } = useCreateModel();

  const handleSubmit = async (formData: ModelFormData) => {
    await createModelAsync({
      ...formData,
      firstAppearanceDate: format(formData.firstAppearanceDate, "yyyy-MM-dd"),
    });

    setIsCreateModelDialogOpen(false);
  };

  return (
    <Dialog
      open={isCreateModelDialogOpen}
      onOpenChange={setIsCreateModelDialogOpen}
    >
      <DialogTrigger asChild>
        <Button>Add model</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create new model</DialogTitle>
        </DialogHeader>
        <ModelForm
          model={{
            name: "",
            firstAppearanceDate: new Date(),
            isDiscontinued: false,
            makeId: "",
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default CreateModelDialog;
