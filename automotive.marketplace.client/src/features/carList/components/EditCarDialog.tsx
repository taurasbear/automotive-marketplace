import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useUpdateCar } from "../api/useUpdateCar";
import { CarFormData } from "../types/CarFormData";
import EditCarDialogContent from "./EditCarDialogContent";

type ViewCarDialogProps = {
  id: string;
};

const EditCarDialog = ({ id }: ViewCarDialogProps) => {
  const [isEditCarDialogOpen, setIsEditCarDialogOpen] =
    useState<boolean>(false);

  const { mutateAsync: updateCarAsync } = useUpdateCar();

  const handleSubmit = async (formData: CarFormData) => {
    await updateCarAsync({
      ...formData,
      id,
    });

    setIsEditCarDialogOpen(false);
  };

  return (
    <Dialog open={isEditCarDialogOpen} onOpenChange={setIsEditCarDialogOpen}>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit car</DialogTitle>
        </DialogHeader>
        <EditCarDialogContent id={id} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default EditCarDialog;
