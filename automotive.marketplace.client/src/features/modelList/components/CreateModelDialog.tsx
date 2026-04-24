import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useCreateModel } from "../api/useCreateModel";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

const CreateModelDialog = () => {
  const [isCreateModelDialogOpen, setIsCreateModelDialogOpen] =
    useState<boolean>();
  const { t } = useTranslation("admin");

  const { mutateAsync: createModelAsync } = useCreateModel();

  const handleSubmit = async (formData: ModelFormData) => {
    await createModelAsync({
      ...formData,
    });

    setIsCreateModelDialogOpen(false);
  };

  return (
    <Dialog
      open={isCreateModelDialogOpen}
      onOpenChange={setIsCreateModelDialogOpen}
    >
      <DialogTrigger asChild>
        <Button>{t("models.addModel")}</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("models.createNewModel")}</DialogTitle>
        </DialogHeader>
        <ModelForm
          model={{
            name: "",
            makeId: "",
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default CreateModelDialog;
