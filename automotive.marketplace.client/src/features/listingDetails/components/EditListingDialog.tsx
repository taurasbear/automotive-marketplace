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
import { useUpdateListing } from "../api/useUpdateListing";
import { GetListingByIdResponse } from "../types/GetListingByIdResponse";
import { UpdateListingFormData } from "../types/UpdateListingFormData";
import EditListingForm from "./EditListingForm";

type ViewListingDialogProps = {
  id: string;
  listing: GetListingByIdResponse;
};

const EditListingDialog = ({ id, listing }: ViewListingDialogProps) => {
  const [isEditListingDialogOpen, setIsEditListingDialogOpen] =
    useState<boolean>(false);

  const { mutateAsync: updateListingAsync } = useUpdateListing();

  const handleSubmit = async (formData: UpdateListingFormData) => {
    await updateListingAsync({
      ...formData,
      id,
    });

    setIsEditListingDialogOpen(false);
  };

  return (
    <Dialog
      open={isEditListingDialogOpen}
      onOpenChange={setIsEditListingDialogOpen}
    >
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit listing</DialogTitle>
        </DialogHeader>
        <EditListingForm listing={listing} id={id} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default EditListingDialog;
